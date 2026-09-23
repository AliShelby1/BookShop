using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IWishlistService
    {
        /// <summary>
        /// Retrieves the entire wishlist for the current user or guest session.
        /// </summary>
        Task<WishlistVM> GetWishlistAsync();

        /// <summary>
        /// Toggles a book in the wishlist: if present, removes it; if absent, adds it.
        /// Ideal for one-click heart icon buttons on book cards and details pages.
        /// </summary>
        Task<WishlistOperationResult> ToggleWishlistAsync(int bookId);

        /// <summary>
        /// Adds a book to the wishlist, strictly preventing duplicates.
        /// </summary>
        Task<WishlistOperationResult> AddToWishlistAsync(int bookId);

        /// <summary>
        /// Removes a book from the wishlist.
        /// </summary>
        Task<WishlistOperationResult> RemoveFromWishlistAsync(int bookId);

        /// <summary>
        /// Moves a book from the Wishlist into the Cart (Reading Bag) using ICartService.
        /// </summary>
        Task<WishlistOperationResult> MoveToBagAsync(int bookId);

        /// <summary>
        /// Returns the total number of books currently saved in the wishlist (for navbar badge).
        /// </summary>
        Task<int> GetWishlistItemCountAsync();

        /// <summary>
        /// Returns a HashSet of Book IDs saved in the current user's wishlist.
        /// Used by catalog and details views to instantly determine which heart buttons should be filled.
        /// </summary>
        Task<HashSet<int>> GetUserWishlistBookIdsAsync();

        /// <summary>
        /// Merges any guest cookie wishlist into the registered user's wishlist upon login or registration.
        /// </summary>
        Task MergeGuestWishlistAsync(string userId);
    }
}
