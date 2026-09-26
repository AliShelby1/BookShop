using BookShop.Data;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;
using BookShop.Services;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    /// <summary>
    /// Manages customer order history and staff order fulfillment operations.
    /// Enforces IDOR security, ownership isolation, and valid status state machine transitions.
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILanguageService _lang;

        public OrderController(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ILanguageService lang)
        {
            _orderService = orderService;
            _userManager = userManager;
            _lang = lang;
        }

        // ── GET /Order (or /Order/Index) ──────────────────────────────────────
        // Displays "My Orders" for customers or "All Orders" for Admin/Staff
        public async Task<IActionResult> Index(OrderStatus? status, string? search)
        {
            var userId = _userManager.GetUserId(User)!;
            bool isStaff = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);

            var vm = await _orderService.GetOrdersListAsync(userId, isStaff, status, search);
            return View(vm);
        }

        // ── GET /Order/Details/{id} ───────────────────────────────────────────
        // Displays itemized snapshot receipt, timeline, and actions
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            bool isStaff = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);

            var order = await _orderService.GetOrderDetailsAsync(id, userId, isStaff);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderDetailsVM
            {
                Order = order,
                IsAdminOrStaff = isStaff
            };

            return View(vm);
        }

        // ── POST /Order/UpdateStatus (Staff Only) ──────────────────────────────
        // Advances the order fulfillment lifecycle (Pending → Confirmed → Preparing → Shipped → Delivered)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            if (success)
            {
                TempData["success"] = _lang.IsRtl
                    ? $"تم تحديث حالة الطلب BSH-{orderId:D6} بنجاح."
                    : $"Order BSH-{orderId:D6} status updated successfully.";
            }
            else
            {
                TempData["error"] = _lang.IsRtl
                    ? "تعذر تحديث حالة الطلب."
                    : "Unable to update order status.";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // ── POST /Order/Cancel/{id} ───────────────────────────────────────────
        // Cancels order and atomically restocks books back to inventory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            bool isStaff = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);

            var success = await _orderService.CancelOrderAsync(id, userId, isStaff);
            if (success)
            {
                TempData["success"] = _lang.IsRtl
                    ? $"تم إلغاء الطلب BSH-{id:D6} وإرجاع الكتب للمخزون."
                    : $"Order BSH-{id:D6} has been cancelled and inventory restocked.";
            }
            else
            {
                TempData["error"] = _lang.IsRtl
                    ? "لا يمكن إلغاء هذا الطلب في حالته الحالية."
                    : "This order cannot be cancelled in its current status.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
