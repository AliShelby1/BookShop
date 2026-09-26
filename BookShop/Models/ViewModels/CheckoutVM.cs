using System.ComponentModel.DataAnnotations;

namespace BookShop.Models.ViewModels
{
    /// <summary>
    /// Carries both the shipping address form fields (validated server-side)
    /// and the read-only cart summary for display on the Review page.
    ///
    /// 📚 LEARNING NOTE — Why a dedicated ViewModel instead of using ApplicationUser?
    ///
    /// Although ApplicationUser already has address fields (StreetAddress, City, etc.),
    /// we do NOT use those directly as the form model. Reasons:
    ///   1. The user may want to ship to a different address than their profile.
    ///   2. Validating address fields independently of the Identity model is cleaner.
    ///   3. We pre-fill these fields FROM the user profile as a convenience,
    ///      but the customer can override them freely at checkout.
    /// </summary>
    public class CheckoutVM
    {
        // ── Shipping Address (form fields, validated) ──────────────────────
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string ShippingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required.")]
        [MaxLength(200)]
        [Display(Name = "Street Address")]
        public string ShippingStreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "State / Province")]
        public string? ShippingState { get; set; }

        [Required(ErrorMessage = "Postal code is required.")]
        [MaxLength(20)]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [MaxLength(100)]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = string.Empty;

        [Phone]
        [MaxLength(30)]
        [Display(Name = "Phone Number (optional)")]
        public string? ShippingPhoneNumber { get; set; }

        [MaxLength(500)]
        [Display(Name = "Order Notes (optional)")]
        public string? CustomerNote { get; set; }

        // ── Read-Only Cart Summary (populated by controller, not bound from form) ──
        public CartVM? Cart { get; set; }
    }
}
