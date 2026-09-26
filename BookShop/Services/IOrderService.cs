using BookShop.Models;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Places a new order atomically:
        /// 1. Creates the OrderHeader with shipping address + financial snapshots
        /// 2. Creates OrderDetail rows with price and title snapshots
        /// 3. Decrements Book.StockQuantity for each item
        /// 4. Clears the customer's Cart
        /// Returns the new OrderHeader.Id on success.
        /// </summary>
        Task<int> PlaceOrderAsync(CheckoutVM checkout, string userId);

        /// <summary>
        /// Returns the order confirmation data for the receipt page.
        /// Only the owning customer or an Admin/Employee can view an order.
        /// Returns null if the order does not exist or is forbidden.
        /// </summary>
        Task<OrderConfirmationVM?> GetOrderConfirmationAsync(int orderId, string userId);

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
