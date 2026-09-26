namespace BookShop.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Order Confirmation page shown after a successful order placement.
    /// This is a read-only receipt — no form fields.
    /// </summary>
    public class OrderConfirmationVM
    {
        public int OrderId { get; set; }
        public string OrderNumber => $"BSH-{OrderId:D6}"; // e.g. BSH-000042
        public DateTime OrderDate { get; set; }

        // Shipping Address
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingStreetAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string? ShippingState { get; set; }
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string? ShippingPhoneNumber { get; set; }

        // Order Lines
        public List<OrderDetailVM> Items { get; set; } = new();

        // Financials
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalDiscountSaved { get; set; }
        public decimal OrderTotal { get; set; }
        public bool IsFreeShipping => ShippingFee == 0 && Subtotal > 0;
    }

    public class OrderDetailVM
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? BookTitleAr { get; set; }
        public string? AuthorName { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : BookTitle;

        public string DisplayCoverImageUrl =>
            !string.IsNullOrWhiteSpace(CoverImageUrl) ? CoverImageUrl : "/images/default-book-cover.svg";
    }
}
