using System.Threading.Tasks;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IReviewService
    {
        Task<BookReviewsSummaryVM> GetBookReviewsSummaryAsync(int bookId, string? currentUserId);

        Task<bool> HasPurchasedBookAsync(string userId, int bookId);

        Task<(bool Success, string Message)> SubmitReviewAsync(ReviewFormVM model, string userId);

        Task<ReviewFormVM?> GetReviewForEditAsync(int reviewId, string userId);

        Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, ReviewFormVM model, string userId);

        Task<(bool Success, string Message)> DeleteReviewAsync(int reviewId, string userId, bool isStaff);

        Task<AdminReviewModerationVM> GetReviewsForModerationAsync(bool? isApproved, string? search);

        Task<bool> ToggleApprovalAsync(int reviewId);
    }
}
