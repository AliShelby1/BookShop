using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Data;

namespace BookShop.Models
{
    /// <summary>
    /// Represents a reader review and rating (1-5 stars) for a book.
    /// Restricted to verified purchasers to maintain editorial integrity and combat review fraud.
    /// </summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Numerical star rating between 1 and 5.
        /// </summary>
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        /// <summary>
        /// Optional short summary / review headline (e.g., "A masterpiece of translation").
        /// </summary>
        [MaxLength(150)]
        public string? Headline { get; set; }

        /// <summary>
        /// In-depth textual reader feedback.
        /// </summary>
        [Required(ErrorMessage = "Review comment is required.")]
        [MinLength(10, ErrorMessage = "Please write at least 10 characters.")]
        [MaxLength(2000, ErrorMessage = "Review cannot exceed 2000 characters.")]
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the reviewer purchased this edition from BookShop.
        /// </summary>
        public bool IsVerifiedPurchase { get; set; } = true;

        /// <summary>
        /// Moderation flag: if false, review is hidden from public display by store curators.
        /// </summary>
        public bool IsApproved { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
