using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    /// <summary>
    /// A single line item within an order. Stores price and title SNAPSHOTS.
    ///
    /// 📚 LEARNING NOTE — Why We Snapshot Price and Title Here:
    ///
    /// Consider this scenario:
    ///   1. Customer orders "Clean Code" at $29.99 on January 1.
    ///   2. Admin changes the book price to $39.99 on February 1.
    ///   3. Customer checks their January receipt on March 1.
    ///
    /// If we stored only BookId and calculated the price from Book.Price,
    /// the receipt would wrongly show $39.99 instead of $29.99.
    ///
    /// By storing UnitPrice and BookTitle directly on the OrderDetail row,
    /// the historical data is immutable — it cannot be accidentally changed
    /// by future edits to the Book catalog. This is non-negotiable in
    /// any real e-commerce system.
    /// </summary>
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        // ── Parent Order ──────────────────────────────────────────────────────
        [Required]
        public int OrderHeaderId { get; set; }

        [ForeignKey(nameof(OrderHeaderId))]
        public OrderHeader? OrderHeader { get; set; }

        // ── Book Reference (nullable: book may be deleted but receipt persists) ─
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        // ── PRICE SNAPSHOT — Locked at order time, never changes ───────────────
        [Required]
        [MaxLength(200)]
        public string BookTitle { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? BookTitleAr { get; set; }

        [MaxLength(100)]
        public string? AuthorName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        // ── Computed ────────────────────────────────────────────────────────────
        [NotMapped]
        public decimal LineTotal => UnitPrice * Quantity;

        public string GetDisplayTitle(bool isRtl) =>
            (isRtl && !string.IsNullOrWhiteSpace(BookTitleAr)) ? BookTitleAr : BookTitle;
    }
}
