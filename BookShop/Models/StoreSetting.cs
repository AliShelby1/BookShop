using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class StoreSetting
    {
        [Key]
        public int Id { get; set; } = 1;

        [Required]
        [Range(0, 10000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Free Shipping Threshold ($)")]
        public decimal FreeShippingThreshold { get; set; } = 50.00m;

        [Required]
        [Range(0, 1000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Flat Shipping Rate ($)")]
        public decimal FlatShippingRate { get; set; } = 4.99m;

        [Required]
        [Range(1, 100)]
        [Display(Name = "Max Quantity Per Book")]
        public int MaxQuantityPerBook { get; set; } = 10;

        [MaxLength(250)]
        [Display(Name = "Cart Announcement / Promotion Banner")]
        public string? CartAnnouncementBanner { get; set; } = "Complimentary literary bookmark & archival packaging on all orders over $50.";

        [MaxLength(250)]
        [Display(Name = "Cart Announcement (Arabic)")]
        public string? CartAnnouncementBannerAr { get; set; } = "تغليف إهدائي فاخر وفاصل كتب مجاني مع كل طلب يتجاوز 50 دولاراً.";

        [MaxLength(10)]
        [Display(Name = "Default Storefront Language")]
        public string DefaultLanguage { get; set; } = "en";

        [Display(Name = "Enable Language Switcher on Storefront")]
        public bool EnableLanguageSwitcher { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
