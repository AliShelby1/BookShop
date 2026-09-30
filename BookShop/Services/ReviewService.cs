using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Data;
using BookShop.Models;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPurchasedBookAsync(string userId, int bookId)
        {
            if (string.IsNullOrWhiteSpace(userId) || bookId <= 0)
                return false;

            return await _context.OrderDetails
                .AnyAsync(od => od.BookId == bookId
                             && od.OrderHeader != null
                             && od.OrderHeader.ApplicationUserId == userId
                             && od.OrderHeader.OrderStatus != OrderStatus.Cancelled);
        }

        public async Task<BookReviewsSummaryVM> GetBookReviewsSummaryAsync(int bookId, string? currentUserId)
        {
            var vm = new BookReviewsSummaryVM
            {
                BookId = bookId
            };

            // Fetch approved reviews, plus current user's unapproved review if applicable
            var reviewsQuery = _context.Reviews
                .Where(r => r.BookId == bookId && (r.IsApproved || (currentUserId != null && r.UserId == currentUserId)))
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt);

            var reviews = await reviewsQuery.ToListAsync();

            var approvedReviews = reviews.Where(r => r.IsApproved).ToList();
            vm.TotalReviews = approvedReviews.Count;
            vm.AverageRating = approvedReviews.Any()
                ? Math.Round(approvedReviews.Average(r => r.Rating), 1)
                : 0.0;

            // Compute Star Distribution
            foreach (var r in approvedReviews)
            {
                if (vm.RatingDistribution.ContainsKey(r.Rating))
                {
                    vm.RatingDistribution[r.Rating]++;
                }
            }

            // Map review view models
            vm.Reviews = reviews.Select(r => new BookReviewItemVM
            {
                Id = r.Id,
                BookId = r.BookId,
                UserId = r.UserId,
                ReviewerName = !string.IsNullOrWhiteSpace(r.User?.Name) ? r.User.Name : (r.User?.UserName ?? "Reader"),
                Rating = r.Rating,
                Headline = r.Headline,
                Comment = r.Comment,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                IsApproved = r.IsApproved,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsCurrentUser = currentUserId != null && r.UserId == currentUserId
            }).ToList();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                vm.IsVerifiedPurchaser = await HasPurchasedBookAsync(currentUserId, bookId);
                var existingReview = vm.Reviews.FirstOrDefault(r => r.UserId == currentUserId);
                vm.CurrentUserReview = existingReview;
                vm.CanUserReview = vm.IsVerifiedPurchaser && existingReview == null;
            }

            return vm;
        }

        public async Task<(bool Success, string Message)> SubmitReviewAsync(ReviewFormVM model, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "You must be signed in to submit a review.");

            // Business Rule: Verified Purchase Enforcement
            var hasPurchased = await HasPurchasedBookAsync(userId, model.BookId);
            if (!hasPurchased)
            {
                return (false, "Only verified readers who have acquired this edition may submit a review.");
            }

            // Check if already reviewed (unique constraint defense)
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.BookId == model.BookId && r.UserId == userId);
            if (alreadyReviewed)
            {
                return (false, "You have already reviewed this book. You may edit your existing review.");
            }

            var review = new Review
            {
                BookId = model.BookId,
                UserId = userId,
                Rating = Math.Clamp(model.Rating, 1, 5),
                Headline = model.Headline?.Trim(),
                Comment = model.Comment.Trim(),
                IsVerifiedPurchase = true,
                IsApproved = true, // By default approved; admin can moderate later
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return (true, "Your review has been published successfully. Thank you for your literary contribution!");
        }

        public async Task<ReviewFormVM?> GetReviewForEditAsync(int reviewId, string userId)
        {
            var review = await _context.Reviews
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null) return null;

            return new ReviewFormVM
            {
                Id = review.Id,
                BookId = review.BookId,
                BookTitle = review.Book.Title,
                BookTitleAr = review.Book.TitleAr,
                CoverImageUrl = review.Book.CoverImageUrl,
                Rating = review.Rating,
                Headline = review.Headline,
                Comment = review.Comment
            };
        }

        public async Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, ReviewFormVM model, string userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
            {
                return (false, "Review not found or you are not authorized to edit it.");
            }

            review.Rating = Math.Clamp(model.Rating, 1, 5);
            review.Headline = model.Headline?.Trim();
            review.Comment = model.Comment.Trim();
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Your review has been updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteReviewAsync(int reviewId, string userId, bool isStaff)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return (false, "Review not found.");
            }

            // Authorization: Author or Staff
            if (!isStaff && review.UserId != userId)
            {
                return (false, "You are not authorized to delete this review.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return (true, "Review removed successfully.");
        }

        public async Task<AdminReviewModerationVM> GetReviewsForModerationAsync(bool? isApproved, string? search)
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsQueryable();

            int total = await query.CountAsync();
            int approved = await query.CountAsync(r => r.IsApproved);
            int hidden = await query.CountAsync(r => !r.IsApproved);

            if (isApproved.HasValue)
            {
                query = query.Where(r => r.IsApproved == isApproved.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r => r.Book.Title.ToLower().Contains(term)
                                      || (r.Book.TitleAr != null && r.Book.TitleAr.ToLower().Contains(term))
                                      || (r.User.Name != null && r.User.Name.ToLower().Contains(term))
                                      || (r.User.Email != null && r.User.Email.ToLower().Contains(term))
                                      || (r.Headline != null && r.Headline.ToLower().Contains(term))
                                      || r.Comment.ToLower().Contains(term));
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminReviewItemVM
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    BookTitleAr = r.Book.TitleAr,
                    CoverImageUrl = r.Book.CoverImageUrl,
                    UserId = r.UserId,
                    ReviewerName = !string.IsNullOrWhiteSpace(r.User.Name) ? r.User.Name : (r.User.UserName ?? "Reader"),
                    ReviewerEmail = r.User.Email ?? string.Empty,
                    Rating = r.Rating,
                    Headline = r.Headline,
                    Comment = r.Comment,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new AdminReviewModerationVM
            {
                Reviews = reviews,
                FilterApproved = isApproved,
                SearchQuery = search,
                TotalCount = total,
                ApprovedCount = approved,
                HiddenCount = hidden
            };
        }

        public async Task<bool> ToggleApprovalAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
