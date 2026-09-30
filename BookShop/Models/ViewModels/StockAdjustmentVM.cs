using System.ComponentModel.DataAnnotations;
using BookShop.Models.Enums;

namespace BookShop.Models.ViewModels
{
    public class StockAdjustmentVM
    {
        [Required]
        public int BookId { get; set; }

        public string? BookTitle { get; set; }
        public string? BookTitleAr { get; set; }
        public string? CoverImageUrl { get; set; }
        public int CurrentStock { get; set; }

        /// <summary>
        /// Quantity change (positive to increase stock, negative to decrease).
        /// </summary>
        [Required(ErrorMessage = "Quantity change is required.")]
        public int QuantityChange { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        public InventoryTransactionType TransactionType { get; set; } = InventoryTransactionType.Purchase;

        [Required(ErrorMessage = "A reason or reference note is required for the audit ledger.")]
        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : (BookTitle ?? string.Empty);
    }
}
