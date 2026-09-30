using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Models.Enums;

namespace BookShop.Models
{
    /// <summary>
    /// Represents an immutable audit entry in the inventory ledger.
    /// Every change to a book's physical quantity generates a transaction row.
    ///
    /// 📚 LEARNING NOTE — The Append-Only Ledger Pattern:
    /// In modern enterprise systems, inventory is NEVER modified without an audit trail.
    /// Instead of simply updating `Book.StockQuantity = new_value`, we record:
    ///   1. Exactly how many units changed (+ or -).
    ///   2. The snapshot of stock before and after the event.
    ///   3. The exact business reason (e.g., "Supplier PO #104", "Customer Order BSH-000001").
    ///   4. Who performed the action and when.
    /// This makes inventory traceable, auditable, and resistant to fraud or data discrepancies.
    /// </summary>
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        // ── Foreign Key to Book ──────────────────────────────────────────────
        [Required]
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        // ── Movement Details ─────────────────────────────────────────────────
        /// <summary>
        /// The signed quantity change (+ for additions/returns, - for sales/losses).
        /// </summary>
        public int QuantityChange { get; set; }

        /// <summary>
        /// Stock quantity snapshot immediately before this transaction.
        /// </summary>
        public int QuantityBefore { get; set; }

        /// <summary>
        /// Stock quantity snapshot immediately after this transaction.
        /// </summary>
        public int QuantityAfter { get; set; }

        /// <summary>
        /// The business classification (Purchase, Sale, Return, Adjustment, Damage).
        /// </summary>
        public InventoryTransactionType TransactionType { get; set; }

        // ── Audit Metadata ───────────────────────────────────────────────────
        [MaxLength(300)]
        public string? Reason { get; set; }

        [Required]
        [MaxLength(150)]
        public string CreatedBy { get; set; } = "System";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
