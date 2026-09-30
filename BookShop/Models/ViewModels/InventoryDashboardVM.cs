namespace BookShop.Models.ViewModels
{
    public class InventoryDashboardVM
    {
        public List<InventoryBookItemVM> Books { get; set; } = new();

        // Metrics Summary
        public int TotalTitlesCount { get; set; }
        public int TotalUnitsInStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Filter / Search
        public string? Filter { get; set; } // "all", "low", "out"
        public string? SearchString { get; set; }
        public int LowStockThreshold { get; set; } = 5;
    }

    public class InventoryBookItemVM
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleAr { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryNameAr { get; set; }
        public string? ISBN { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsActive { get; set; }

        public bool IsOutOfStock => StockQuantity <= 0;
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 5;

        public string DisplayCoverImageUrl =>
            !string.IsNullOrWhiteSpace(CoverImageUrl) ? CoverImageUrl : "/images/default-book-cover.svg";

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(TitleAr)) ? TitleAr : Title;

        public string GetDisplayCategory(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(CategoryNameAr)) ? CategoryNameAr : (CategoryName ?? string.Empty);
    }
}
