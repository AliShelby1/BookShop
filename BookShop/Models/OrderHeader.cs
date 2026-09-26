using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Data;
using BookShop.Models.Enums;

namespace BookShop.Models
{
    /// <summary>
    /// The master record for a placed order.
    /// 
    /// 📚 LEARNING NOTE — Supporting Guest Checkout & Immutability:
    /// 
    /// 1. Nullable ApplicationUserId:
    ///    When ApplicationUserId is nullable (string? instead of string),
    ///    guests can complete a purchase without creating an account first.
    ///    This drastically reduces shopping cart abandonment.
    /// 
    /// 2. CustomerEmail:
    ///    Mandatory for ALL orders (guests and registered users).
    ///    For guests, this is the primary identity anchor for dispatching receipts
    ///    and tracking updates.
    /// 
    /// 3. OrderGuid (Secure Token):
    ///    A unique cryptographically random token assigned to every order.
    ///    This allows guests to view their order confirmation securely
    ///    without needing a password-protected account.
    /// </summary>
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        // ── Customer Reference (Nullable for Guest Checkout) ─────────────────
        public string? ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string CustomerEmail { get; set; } = string.Empty;

        // Secure access GUID token for guest confirmation lookup
        public Guid OrderGuid { get; set; } = Guid.NewGuid();

        // Optional snapshot of guest session cart cookie
        [MaxLength(100)]
        public string? SessionCartId { get; set; }

        // ── Shipping Address Snapshot (copied from form at checkout time) ────
        [Required]
        [MaxLength(100)]
        public string ShippingName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ShippingStreetAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ShippingCity { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShippingState { get; set; }

        [Required]
        [MaxLength(20)]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ShippingCountry { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? ShippingPhoneNumber { get; set; }

        // ── Financial Snapshot (totals locked at order time) ─────────────────
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscountSaved { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        // ── Status ───────────────────────────────────────────────────────────
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // ── Notes & Special Instructions ─────────────────────────────────────
        [MaxLength(500)]
        public string? CustomerNote { get; set; }

        // ── Timestamps ───────────────────────────────────────────────────────
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // ── Navigation ───────────────────────────────────────────────────────
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
