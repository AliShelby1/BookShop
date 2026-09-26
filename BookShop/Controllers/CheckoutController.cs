using BookShop.Data;
using BookShop.Models.ViewModels;
using BookShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    /// <summary>
    /// Manages the checkout flow: Address → Review → Place Order → Confirmation.
    /// Supports both authenticated users and guests.
    /// </summary>
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        // ── GET /Checkout/Address ─────────────────────────────────────────────
        // Shows the shipping address form (pre-filled if logged in, blank for guests)
        public async Task<IActionResult> Address()
        {
            var cartVM = await _cartService.GetCartAsync();

            if (cartVM.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var user = User.Identity?.IsAuthenticated == true
                ? await _userManager.GetUserAsync(User)
                : null;

            // Pre-fill fields if user is authenticated
            var vm = new CheckoutVM
            {
                CustomerEmail = user?.Email ?? string.Empty,
                ShippingName = user?.Name ?? string.Empty,
                ShippingStreetAddress = user?.StreetAddress ?? string.Empty,
                ShippingCity = user?.City ?? string.Empty,
                ShippingState = user?.State,
                ShippingPostalCode = user?.PostalCode ?? string.Empty,
                ShippingCountry = string.Empty,
                ShippingPhoneNumber = user?.PhoneNumber,
                Cart = cartVM
            };

            return View(vm);
        }

        // ── POST /Checkout/Address → redirect to Review ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Address(CheckoutVM vm)
        {
            // Attach cart data (not bound from form)
            vm.Cart = await _cartService.GetCartAsync();

            if (vm.Cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
                return View(vm);

            // Store address & contact info in TempData for the Review step
            TempData["CustomerEmail"] = vm.CustomerEmail;
            TempData["ShippingName"] = vm.ShippingName;
            TempData["ShippingStreetAddress"] = vm.ShippingStreetAddress;
            TempData["ShippingCity"] = vm.ShippingCity;
            TempData["ShippingState"] = vm.ShippingState;
            TempData["ShippingPostalCode"] = vm.ShippingPostalCode;
            TempData["ShippingCountry"] = vm.ShippingCountry;
            TempData["ShippingPhoneNumber"] = vm.ShippingPhoneNumber;
            TempData["CustomerNote"] = vm.CustomerNote;

            return RedirectToAction(nameof(Review));
        }

        // ── GET /Checkout/Review ──────────────────────────────────────────────
        // Shows the full order summary (items + contact + address + totals)
        public async Task<IActionResult> Review()
        {
            if (TempData["ShippingName"] == null)
                return RedirectToAction(nameof(Address));

            var cartVM = await _cartService.GetCartAsync();
            if (cartVM.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutVM
            {
                CustomerEmail = TempData["CustomerEmail"]?.ToString() ?? string.Empty,
                ShippingName = TempData["ShippingName"]?.ToString() ?? string.Empty,
                ShippingStreetAddress = TempData["ShippingStreetAddress"]?.ToString() ?? string.Empty,
                ShippingCity = TempData["ShippingCity"]?.ToString() ?? string.Empty,
                ShippingState = TempData["ShippingState"]?.ToString(),
                ShippingPostalCode = TempData["ShippingPostalCode"]?.ToString() ?? string.Empty,
                ShippingCountry = TempData["ShippingCountry"]?.ToString() ?? string.Empty,
                ShippingPhoneNumber = TempData["ShippingPhoneNumber"]?.ToString(),
                CustomerNote = TempData["CustomerNote"]?.ToString(),
                Cart = cartVM
            };

            TempData.Keep();

            return View(vm);
        }

        // ── POST /Checkout/PlaceOrder ──────────────────────────────────────────
        // Atomically creates the order (guest or user), decrements stock, and clears the cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            if (TempData["ShippingName"] == null)
                return RedirectToAction(nameof(Address));

            var vm = new CheckoutVM
            {
                CustomerEmail = TempData["CustomerEmail"]?.ToString() ?? string.Empty,
                ShippingName = TempData["ShippingName"]?.ToString() ?? string.Empty,
                ShippingStreetAddress = TempData["ShippingStreetAddress"]?.ToString() ?? string.Empty,
                ShippingCity = TempData["ShippingCity"]?.ToString() ?? string.Empty,
                ShippingState = TempData["ShippingState"]?.ToString(),
                ShippingPostalCode = TempData["ShippingPostalCode"]?.ToString() ?? string.Empty,
                ShippingCountry = TempData["ShippingCountry"]?.ToString() ?? string.Empty,
                ShippingPhoneNumber = TempData["ShippingPhoneNumber"]?.ToString(),
                CustomerNote = TempData["CustomerNote"]?.ToString()
            };

            // Get userId if authenticated; null for guest orders
            var userId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            try
            {
                var order = await _orderService.PlaceOrderAsync(vm, userId);
                return RedirectToAction(nameof(Confirmation), new { id = order.Id, token = order.OrderGuid });
            }
            catch (InvalidOperationException ex)
            {
                TempData["OrderError"] = ex.Message;
                TempData.Keep();
                return RedirectToAction(nameof(Review));
            }
        }

        // ── GET /Checkout/Confirmation/{id}?token={token} ────────────────────
        // Displays receipt for customer or guest (verified via token)
        public async Task<IActionResult> Confirmation(int id, Guid? token)
        {
            var userId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var vm = await _orderService.GetOrderConfirmationAsync(id, userId, token);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
    }
}
