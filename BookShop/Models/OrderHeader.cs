using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Data;
using BookShop.Models.Enums;

namespace BookShop.Models
{
    /// <summary>
    /// The master record for a placed order.
    /// 
    /// 📚 LEARNING NOTE — Shipping Address Denormalization:
    /// We copy the shipping address fields directly onto the OrderHeader instead of
    /// referencing a separate Address table. This is intentional: if the customer
    /// later updates their profile address, old order receipts must still show the
    /// exact address that was used at the time of purchase. This is the same
    /// "snapshot" principle that applies to price data in OrderDetail.
    /// </summary>
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        // ── Customer Reference ───────────────────────────────────────────────
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

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
