using BookShop.Data;
using BookShop.Models;
using BookShop.Models.ViewModels;
using BookShop.Services;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class StorefrontController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;

        public StorefrontController(ApplicationDbContext db, ICartService cartService)
        {
            _db = db;
            _cartService = cartService;
        }

        // GET: /Storefront
        public async Task<IActionResult> Index()
        {
            var activeBooks = await _db.Books
                .Where(b => b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();

            var currentVolume = activeBooks.FirstOrDefault(b => b.IsVolumeOfTheMonth);
            var settings = await _cartService.GetStoreSettingsAsync();

            var vm = new StorefrontCuratorVM
            {
                CurrentVolumeOfTheMonth = currentVolume,
                SelectedVolumeId = currentVolume?.Id,
                AllActiveBooks = activeBooks,
                Categories = await _db.Categories.OrderBy(c => c.DisplayOrder).ToListAsync(),
                FeaturedBookIds = activeBooks.Where(b => b.IsFeatured).Select(b => b.Id).ToList(),
                StoreSettings = settings
            };

            return View(vm);
        }

        // POST: /Storefront/SetVolumeOfTheMonth
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetVolumeOfTheMonth(int bookId)
        {
            var selectedBook = await _db.Books.FindAsync(bookId);
            if (selectedBook == null || !selectedBook.IsActive)
            {
                TempData["error"] = "Selected book could not be found or is inactive.";
                return RedirectToAction(nameof(Index));
            }

            // Reset any existing Volume of the Month
            var allVolumes = await _db.Books.Where(b => b.IsVolumeOfTheMonth).ToListAsync();
            foreach (var b in allVolumes)
            {
                b.IsVolumeOfTheMonth = false;
            }

            // Set new selection
            selectedBook.IsVolumeOfTheMonth = true;
            await _db.SaveChangesAsync();

            TempData["success"] = $"'{selectedBook.Title}' is now the spotlighted Volume of the Month on the Home Page!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Storefront/ToggleFeatured
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int bookId)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["error"] = "Book not found.";
                return RedirectToAction(nameof(Index));
            }

            book.IsFeatured = !book.IsFeatured;
            await _db.SaveChangesAsync();

            TempData["success"] = $"'{book.Title}' is now {(book.IsFeatured ? "included in" : "removed from")} Featured Editions.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Storefront/UpdateCategoryIcon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategoryIcon(int categoryId, string iconClass)
        {
            var category = await _db.Categories.FindAsync(categoryId);
            if (category == null)
            {
                TempData["error"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            category.IconClass = string.IsNullOrWhiteSpace(iconClass) ? "bi-book" : iconClass.Trim();
            await _db.SaveChangesAsync();

            TempData["success"] = $"Updated icon for '{category.Name}' to '{category.IconClass}'.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Storefront/UpdateStoreSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStoreSettings(StoreSetting storeSettings)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please check the entered values for store policies.";
                return RedirectToAction(nameof(Index));
            }

            await _cartService.UpdateStoreSettingsAsync(storeSettings);
            TempData["success"] = "Store and cart parameters updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
