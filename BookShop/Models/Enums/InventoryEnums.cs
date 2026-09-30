namespace BookShop.Models.Enums
{
    /// <summary>
    /// Represents the business reason and movement direction for an inventory transaction.
    /// </summary>
    public enum InventoryTransactionType
    {
        /// <summary>
        /// Incoming stock from a publisher, supplier, or acquisition (increases stock).
        /// </summary>
        Purchase = 1,

        /// <summary>
        /// Stock deducted due to a customer checkout order (decreases stock).
        /// </summary>
        Sale = 2,

        /// <summary>
        /// Stock restored due to an order cancellation or customer return (increases stock).
        /// </summary>
        Return = 3,

        /// <summary>
        /// Manual inventory count correction or reconciliation (positive or negative).
        /// </summary>
        Adjustment = 4,

        /// <summary>
        /// Damaged, defective, or lost book written off (decreases stock).
        /// </summary>
        Damage = 5
    }
}
