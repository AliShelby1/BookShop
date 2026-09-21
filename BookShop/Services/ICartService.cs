using BookShop.Models;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface ICartService
    {
        Task<CartVM> GetCartAsync();
        Task<CartOperationResult> AddToCartAsync(int bookId, int quantity = 1);
        Task<CartOperationResult> UpdateQuantityAsync(int cartItemId, int newQuantity);
        Task<CartOperationResult> RemoveItemAsync(int cartItemId);
        Task ClearCartAsync();
        Task<int> GetCartItemCountAsync();
        Task MergeGuestCartAsync(string userId);
        Task<StoreSetting> GetStoreSettingsAsync();
        Task UpdateStoreSettingsAsync(StoreSetting settings);
    }
}
