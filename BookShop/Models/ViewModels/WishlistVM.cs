namespace BookShop.Models.ViewModels
{
    public class WishlistItemVM
    {
        public int WishlistItemId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleAr { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryNameAr { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddedAt { get; set; }

        public bool IsInStock => IsActive && StockQuantity > 0;

        public string GetDisplayTitle(bool isArabic)
        {
            return isArabic && !string.IsNullOrWhiteSpace(TitleAr) ? TitleAr : Title;
        }

        public string DisplayCoverImageUrl => !string.IsNullOrWhiteSpace(CoverImageUrl)
            ? CoverImageUrl
            : "/images/default-book-cover.svg";
    }

    public class WishlistVM
    {
        public List<WishlistItemVM> Items { get; set; } = new();
        public int TotalItems => Items.Count;
        public bool IsEmpty => Items.Count == 0;
    }
}
