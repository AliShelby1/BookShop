using System;
using System.Collections.Generic;
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

        // Native PostgreSQL JSONB dictionary for multilingual titles
        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> TitleTranslations { get; set; } = new();

        [NotMapped]
        [Display(Name = "Arabic Title")]
        public string? TitleAr
        {
            get => TitleTranslations.GetValueOrDefault("ar");
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    TitleTranslations["ar"] = value.Trim();
                else
                    TitleTranslations.Remove("ar");
            }
        }

        [Required]
        public string Description { get; set; } = string.Empty;

        // Native PostgreSQL JSONB dictionary for multilingual descriptions
        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> DescriptionTranslations { get; set; } = new();

        [NotMapped]
        [Display(Name = "Arabic Description")]
        public string? DescriptionAr
        {
            get => DescriptionTranslations.GetValueOrDefault("ar");
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    DescriptionTranslations["ar"] = value.Trim();
                else
                    DescriptionTranslations.Remove("ar");
            }
        }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Book Language")]
        public string Language { get; set; } = "English";

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

        // Multilingual display helpers
        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && TitleTranslations.TryGetValue("ar", out var ar) && !string.IsNullOrWhiteSpace(ar)) ? ar : Title;

        public string GetDisplayTitle(string culture) =>
            (TitleTranslations.TryGetValue(culture, out var val) && !string.IsNullOrWhiteSpace(val)) ? val : Title;

        public string GetDisplayDescription(bool isRtl) =>
            (isRtl && DescriptionTranslations.TryGetValue("ar", out var ar) && !string.IsNullOrWhiteSpace(ar)) ? ar : Description;

        public string GetDisplayDescription(string culture) =>
            (DescriptionTranslations.TryGetValue(culture, out var val) && !string.IsNullOrWhiteSpace(val)) ? val : Description;

        public string GetDisplayLanguage(bool isRtl) => isRtl ? (Language switch
        {
            "English" => "الإنجليزية",
            "Arabic" => "العربية",
            "Bilingual" or "Both" or "Arabic / English" => "ثنائي اللغة (عربي / إنجليزي)",
            "French" => "الفرنسية",
            "German" => "الألمانية",
            "Spanish" => "الإسبانية",
            _ => Language
        }) : Language;
    }
}
