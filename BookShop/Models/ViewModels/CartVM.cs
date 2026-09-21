namespace BookShop.Models.ViewModels
{
    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.LineTotal);

        public decimal TotalDiscountSaved => Items.Sum(i => i.TotalDiscountSaved);

        public decimal FreeShippingThreshold { get; set; } = 50.00m;

        public decimal FlatShippingRate { get; set; } = 4.99m;

        public bool IsFreeShipping => Subtotal >= FreeShippingThreshold && Subtotal > 0;

        public decimal ShippingFee => (Subtotal == 0 || IsFreeShipping) ? 0m : FlatShippingRate;

        public decimal AmountNeededForFreeShipping => (Subtotal >= FreeShippingThreshold || Subtotal == 0)
            ? 0m
            : FreeShippingThreshold - Subtotal;

        public double FreeShippingProgressPercentage => FreeShippingThreshold <= 0 
            ? 100 
            : Math.Min(100, Math.Round((double)(Subtotal / FreeShippingThreshold) * 100, 1));

        public decimal Total => Subtotal + ShippingFee;

        public int TotalItemsCount => Items.Sum(i => i.Quantity);

        public string? CartAnnouncementBanner { get; set; }
    }
}
