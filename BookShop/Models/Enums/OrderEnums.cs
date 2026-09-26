namespace BookShop.Models.Enums
{
    /// <summary>
    /// Represents the fulfilment lifecycle of an order.
    /// Status transitions should follow the defined flow:
    ///   Pending → Confirmed → Preparing → Shipped → Delivered
    ///   Any status → Cancelled (within policy window)
    /// </summary>
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Preparing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6
    }

    /// <summary>
    /// Represents the payment status of an order.
    /// Payment is simulated locally (no real gateway) in Phase 6.
    /// A real payment provider (Stripe, PayPal) can replace this in a later phase.
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }
}
