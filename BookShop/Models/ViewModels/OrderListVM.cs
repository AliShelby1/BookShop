using BookShop.Models;
using BookShop.Models.Enums;

namespace BookShop.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Order Listing (serves both customer "My Orders" and Admin "All Orders").
    /// </summary>
    public class OrderListVM
    {
        public List<OrderHeader> Orders { get; set; } = new();

        public OrderStatus? StatusFilter { get; set; }

        public string? SearchString { get; set; }

        public bool IsAdminOrStaff { get; set; }

        // Counts for status tabs
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int PreparingCount { get; set; }
        public int ShippedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CancelledCount { get; set; }
    }
}
