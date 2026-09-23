namespace BookShop.Services
{
    public class WishlistOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool InWishlist { get; set; }
        public int WishlistCount { get; set; }
        public int CartCount { get; set; }

        public static WishlistOperationResult Ok(string message, bool inWishlist, int wishlistCount, int cartCount = 0)
        {
            return new WishlistOperationResult
            {
                Success = true,
                Message = message,
                InWishlist = inWishlist,
                WishlistCount = wishlistCount,
                CartCount = cartCount
            };
        }

        public static WishlistOperationResult Fail(string message)
        {
            return new WishlistOperationResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
