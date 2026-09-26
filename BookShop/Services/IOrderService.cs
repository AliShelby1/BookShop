using BookShop.Models;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Places a new order atomically (supports both logged-in users and guests):
        /// 1. Creates the OrderHeader with shipping address + financial snapshots
        /// 2. Creates OrderDetail rows with price and title snapshots
        /// 3. Decrements Book.StockQuantity for each item
        /// 4. Clears the customer's Cart
        /// Returns the created OrderHeader on success.
        /// </summary>
        Task<OrderHeader> PlaceOrderAsync(CheckoutVM checkout, string? userId, string? sessionCartId = null);

        /// <summary>
        /// Returns the order confirmation data for the receipt page.
        /// Authenticated users can view their own orders. Staff can view all orders.
        /// Guests can view their order by providing their matching orderGuid token.
        /// Returns null if the order does not exist or access is forbidden.
        /// </summary>
        Task<OrderConfirmationVM?> GetOrderConfirmationAsync(int orderId, string? userId, Guid? orderGuid = null);

        /// <summary>
        /// Returns all orders for a specific customer (for the "My Orders" page in Phase 7).
        /// </summary>
        Task<List<OrderHeader>> GetOrdersByUserAsync(string userId);

        /// <summary>
        /// Returns all orders across all customers (for the Admin order management page in Phase 7).
        /// </summary>
        Task<List<OrderHeader>> GetAllOrdersAsync();
    }
}
