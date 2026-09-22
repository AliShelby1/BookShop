using System.Globalization;

namespace BookShop.Utility
{
    public static class PriceExtensions
    {
        public static string ToPrice(this decimal value) => $"${value.ToString("0.00", CultureInfo.InvariantCulture)}";
        public static string ToPrice(this decimal? value) => value.HasValue ? value.Value.ToPrice() : "$0.00";
    }
}
