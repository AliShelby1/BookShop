namespace BookShop.Models.ViewModels
{
    public class CartItemVM
    {
        public int CartItemId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public bool IsInStock => StockQuantity >= Quantity;

        public decimal LineTotal => UnitPrice * Quantity;
        public decimal TotalDiscountSaved => (OriginalPrice - UnitPrice) * Quantity;
    }
}
