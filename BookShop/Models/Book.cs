using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercentage { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "Stock quantity cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Volume of the Month")]
        public bool IsVolumeOfTheMonth { get; set; } = false;

        [Display(Name = "Featured Edition")]
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Key Relationships
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        [Display(Name = "Publisher")]
        public int? PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        // Computed helper property for discounted price (not mapped to DB)
        [NotMapped]
        public decimal EffectivePrice => DiscountPercentage.HasValue && DiscountPercentage.Value > 0
            ? Math.Round(Price * (1 - (DiscountPercentage.Value / 100m)), 2)
            : Price;
    }
}
