using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        // Native PostgreSQL JSONB dictionary for multilingual support
        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> Translations { get; set; } = new();

        [NotMapped]
        [Display(Name = "Arabic Category Name")]
        public string? NameAr
        {
            get => Translations.GetValueOrDefault("ar");
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    Translations["ar"] = value.Trim();
                else
                    Translations.Remove("ar");
            }
        }

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100")]
        public int DisplayOrder { get; set; }

        [MaxLength(50)]
        [Display(Name = "Icon Class")]
        public string? IconClass { get; set; } = "bi-book";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper to return Arabic or specified culture name when in RTL/locale mode
        public string GetDisplayName(bool isRtl) =>
            (isRtl && Translations.TryGetValue("ar", out var ar) && !string.IsNullOrWhiteSpace(ar)) ? ar : Name;

        public string GetDisplayName(string culture) =>
            (Translations.TryGetValue(culture, out var val) && !string.IsNullOrWhiteSpace(val)) ? val : Name;

        // Navigation property
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
