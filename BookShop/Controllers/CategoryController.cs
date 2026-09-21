using BookShop.Data;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Category
        public async Task<IActionResult> Index()
        {
            var categoryList = await _db.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categoryList);
        }

        // GET: /Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            // Check for duplicate category name
            if (await _db.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Category '{category.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            // Check for duplicate category name on other records
            if (await _db.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id))
            {
                ModelState.AddModelError("Name", "Another category with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Category '{category.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["error"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Referential Integrity Safeguard: check if category has books assigned
            var bookCount = await _db.Books.CountAsync(b => b.CategoryId == id);
            if (bookCount > 0)
            {
                TempData["error"] = $"Cannot delete category '{category.Name}' because it currently has {bookCount} book(s) assigned to it. Please reassign or delete those books first.";
                return RedirectToAction(nameof(Index));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["success"] = $"Category '{category.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
