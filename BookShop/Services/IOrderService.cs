using BookShop.Models;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;

namespace BookShop.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Places a new order atomically (supports both logged-in users and guests).
        /// </summary>
        Task<OrderHeader> PlaceOrderAsync(CheckoutVM checkout, string? userId, string? sessionCartId = null);

        /// <summary>
        /// Returns the order confirmation data for the receipt page.
        /// </summary>
        Task<OrderConfirmationVM?> GetOrderConfirmationAsync(int orderId, string? userId, Guid? orderGuid = null);

        /// <summary>
        /// Retrieves filtered and counted order list for customer or admin/employee.
        /// </summary>
        Task<OrderListVM> GetOrdersListAsync(string? userId, bool isStaff, OrderStatus? status = null, string? search = null);

        /// <summary>
        /// Retrieves complete order details with snapshot items, enforcing customer ownership isolation.
        /// </summary>
        Task<OrderHeader?> GetOrderDetailsAsync(int orderId, string? userId, bool isStaff);

        /// <summary>
        /// Advances or transitions an order to a new fulfillment status (staff only).
        /// Stamped with ShippedDate / DeliveredDate when appropriate.
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);

        /// <summary>
        /// Cancels an order and atomically restores inventory stock.
        /// Enforces business rules: customers can only cancel Pending/Confirmed orders.
        /// </summary>
        Task<bool> CancelOrderAsync(int orderId, string? userId, bool isStaff);

        /// <summary>
        /// Returns all orders for a specific customer.
        /// </summary>
        Task<List<OrderHeader>> GetOrdersByUserAsync(string userId);

        /// <summary>
        /// Returns all orders across all customers.
        /// </summary>
        Task<List<OrderHeader>> GetAllOrdersAsync();
    }
}
