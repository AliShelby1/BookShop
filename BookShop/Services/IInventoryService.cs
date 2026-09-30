using BookShop.Models.Enums;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IInventoryService
    {
        /// <summary>
        /// Retrieves the inventory dashboard with metrics, low-stock alerts, and book stock levels.
        /// </summary>
        Task<InventoryDashboardVM> GetDashboardAsync(string? filter = null, string? search = null);

        /// <summary>
        /// Prepares the adjustment form data for a specific book.
        /// </summary>
        Task<StockAdjustmentVM?> GetStockAdjustmentModelAsync(int bookId);

        /// <summary>
        /// Atomically adjusts a book's physical stock level and logs an immutable InventoryTransaction.
        /// </summary>
        Task<(bool Success, string Message)> AdjustStockAsync(StockAdjustmentVM model, string createdBy);

        /// <summary>
        /// Retrieves the chronological audit ledger of inventory movements.
        /// </summary>
        Task<InventoryHistoryVM> GetHistoryAsync(int? bookId = null, InventoryTransactionType? type = null, string? search = null);

        /// <summary>
        /// Records an audit transaction entry directly (used by Checkout & Cancel flows).
        /// </summary>
        Task RecordTransactionAsync(int bookId, int quantityChange, int quantityBefore, int quantityAfter, InventoryTransactionType type, string reason, string createdBy);
    }
}
