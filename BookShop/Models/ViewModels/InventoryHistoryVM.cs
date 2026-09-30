using BookShop.Models.Enums;

namespace BookShop.Models.ViewModels
{
    public class InventoryHistoryVM
    {
        public List<InventoryTransactionItemVM> Transactions { get; set; } = new();

        public int? SelectedBookId { get; set; }
        public string? SelectedBookTitle { get; set; }
        public InventoryTransactionType? SelectedType { get; set; }
        public string? SearchQuery { get; set; }

        public int TotalTransactionsCount { get; set; }
    }

    public class InventoryTransactionItemVM
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? BookTitleAr { get; set; }
        public int QuantityChange { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public string? Reason { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : BookTitle;
    }
}
