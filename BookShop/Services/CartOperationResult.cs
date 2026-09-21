namespace BookShop.Services
{
    public class CartOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalItemsCount { get; set; }

        public static CartOperationResult Ok(string message, int count = 0)
        {
            return new CartOperationResult { Success = true, Message = message, TotalItemsCount = count };
        }

        public static CartOperationResult Fail(string message)
        {
            return new CartOperationResult { Success = false, Message = message };
        }
    }
}
