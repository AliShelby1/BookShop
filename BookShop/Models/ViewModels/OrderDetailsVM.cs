using BookShop.Models;
using BookShop.Models.Enums;

namespace BookShop.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Order Details page with snapshot receipts, timeline, and status progression controls.
    /// </summary>
    public class OrderDetailsVM
    {
        public OrderHeader Order { get; set; } = null!;

        public bool IsAdminOrStaff { get; set; }

        public bool CanCustomerCancel => 
            !IsAdminOrStaff && (Order.OrderStatus == OrderStatus.Pending || Order.OrderStatus == OrderStatus.Confirmed);

        public bool CanAdminCancel => 
            IsAdminOrStaff && Order.OrderStatus != OrderStatus.Delivered && Order.OrderStatus != OrderStatus.Cancelled;

        public OrderStatus? NextStatus => Order.OrderStatus switch
        {
            OrderStatus.Pending => OrderStatus.Confirmed,
            OrderStatus.Confirmed => OrderStatus.Preparing,
            OrderStatus.Preparing => OrderStatus.Shipped,
            OrderStatus.Shipped => OrderStatus.Delivered,
            _ => null
        };
    }
}
