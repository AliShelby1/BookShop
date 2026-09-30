using BookShop.Data;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;
using BookShop.Services;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    /// <summary>
    /// Manages physical inventory levels, low-stock warnings, manual adjustments,
    /// and the immutable audit ledger. Restricted to Administrators and Employees.
    /// </summary>
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILanguageService _lang;

        public InventoryController(
            IInventoryService inventoryService,
            UserManager<ApplicationUser> userManager,
            ILanguageService lang)
        {
            _inventoryService = inventoryService;
            _userManager = userManager;
            _lang = lang;
        }

        // ── GET /Inventory ────────────────────────────────────────────────────
        // Displays the Inventory Dashboard with live stock balances and low-stock alerts
        public async Task<IActionResult> Index(string? filter, string? search)
        {
            var vm = await _inventoryService.GetDashboardAsync(filter, search);
            return View(vm);
        }

        // ── GET /Inventory/Adjust/{id} ─────────────────────────────────────────
        // Displays the adjustment form for a specific book
        public async Task<IActionResult> Adjust(int id)
        {
            var vm = await _inventoryService.GetStockAdjustmentModelAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        // ── POST /Inventory/Adjust ────────────────────────────────────────────
        // Atomically updates stock level and logs an InventoryTransaction entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(StockAdjustmentVM model)
        {
            if (!ModelState.IsValid)
            {
                var refreshed = await _inventoryService.GetStockAdjustmentModelAsync(model.BookId);
                if (refreshed != null)
                {
                    model.BookTitle = refreshed.BookTitle;
                    model.BookTitleAr = refreshed.BookTitleAr;
                    model.CoverImageUrl = refreshed.CoverImageUrl;
                    model.CurrentStock = refreshed.CurrentStock;
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var staffName = user?.Name ?? user?.Email ?? User.Identity?.Name ?? "Staff";

            var (success, message) = await _inventoryService.AdjustStockAsync(model, staffName);

            if (success)
            {
                TempData["success"] = message;
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = message;
            var currentBook = await _inventoryService.GetStockAdjustmentModelAsync(model.BookId);
            if (currentBook != null)
            {
                model.BookTitle = currentBook.BookTitle;
                model.BookTitleAr = currentBook.BookTitleAr;
                model.CoverImageUrl = currentBook.CoverImageUrl;
                model.CurrentStock = currentBook.CurrentStock;
            }
            return View(model);
        }

        // ── GET /Inventory/History ────────────────────────────────────────────
        // Chronological audit ledger of all stock modifications
        public async Task<IActionResult> History(int? bookId, InventoryTransactionType? type, string? search)
        {
            var vm = await _inventoryService.GetHistoryAsync(bookId, type, search);
            return View(vm);
        }
    }
}
