using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Arabic Category Name")]
        public string? NameAr { get; set; }

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100")]
        public int DisplayOrder { get; set; }

        [MaxLength(50)]
        [Display(Name = "Icon Class")]
        public string? IconClass { get; set; } = "bi-book";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper to return Arabic name when in RTL mode if present
        public string GetDisplayName(bool isRtl) => (isRtl && !string.IsNullOrWhiteSpace(NameAr)) ? NameAr : Name;

        // Navigation property
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
