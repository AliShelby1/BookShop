using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models.ViewModels
{
    /// <summary>
    /// ViewModel for submitting or updating a reader review.
    /// </summary>
    public class ReviewFormVM
    {
        public int? Id { get; set; }

        [Required]
        public int BookId { get; set; }

        public string? BookTitle { get; set; }
        public string? BookTitleAr { get; set; }
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a rating between 1 and 5 stars.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; } = 5;

        [MaxLength(150, ErrorMessage = "Headline cannot exceed 150 characters.")]
        public string? Headline { get; set; }

        [Required(ErrorMessage = "Please share your thoughts in the review comment.")]
        [MinLength(10, ErrorMessage = "Review must be at least 10 characters long.")]
        [MaxLength(2000, ErrorMessage = "Review cannot exceed 2000 characters.")]
        public string Comment { get; set; } = string.Empty;

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : (BookTitle ?? string.Empty);
    }

    /// <summary>
    /// ViewModel representing a single customer review in public listings.
    /// </summary>
    public class BookReviewItemVM
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Headline { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    /// <summary>
    /// Aggregated rating metrics and review collection for a specific book edition.
    /// </summary>
    public class BookReviewsSummaryVM
    {
        public int BookId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        /// <summary>
        /// Key: Star count (1..5), Value: Number of reviews with that star count.
        /// </summary>
        public Dictionary<int, int> RatingDistribution { get; set; } = new()
        {
            [5] = 0,
            [4] = 0,
            [3] = 0,
            [2] = 0,
            [1] = 0
        };

        public List<BookReviewItemVM> Reviews { get; set; } = new();

        /// <summary>
        /// True if the current user is authenticated AND purchased this book, and has not yet reviewed it.
        /// </summary>
        public bool CanUserReview { get; set; }

        /// <summary>
        /// True if the user is authenticated and has purchased the book.
        /// </summary>
        public bool IsVerifiedPurchaser { get; set; }

        /// <summary>
        /// The user's existing review if they already submitted one.
        /// </summary>
        public BookReviewItemVM? CurrentUserReview { get; set; }

        public int GetRatingPercentage(int star)
        {
            if (TotalReviews == 0) return 0;
            if (RatingDistribution.TryGetValue(star, out int count))
            {
                return (int)Math.Round((double)count / TotalReviews * 100);
            }
            return 0;
        }
    }

    /// <summary>
    /// Admin moderation item for managing reader reviews.
    /// </summary>
    public class AdminReviewItemVM
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string? BookTitleAr { get; set; }
        public string? CoverImageUrl { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Headline { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : BookTitle;
    }

    /// <summary>
    /// Admin review moderation dashboard model with filtering and metrics.
    /// </summary>
    public class AdminReviewModerationVM
    {
        public List<AdminReviewItemVM> Reviews { get; set; } = new();
        public bool? FilterApproved { get; set; }
        public string? SearchQuery { get; set; }

        public int TotalCount { get; set; }
        public int ApprovedCount { get; set; }
        public int HiddenCount { get; set; }
    }
}
