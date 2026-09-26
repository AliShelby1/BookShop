using BookShop.Data;
using BookShop.Models.ViewModels;
using BookShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    /// <summary>
    /// Manages the checkout flow: Address → Review → Place Order → Confirmation.
    ///
    /// 📚 LEARNING NOTE — [Authorize]:
    /// The entire checkout process requires a logged-in user because we need to:
    ///   1. Associate the order with a real ApplicationUser (for order history)
    ///   2. Pre-fill the address form from the user's profile
    ///   3. Send a confirmation email (in a later phase)
    ///
    /// Guests must log in or register before placing an order — this is standard
    /// e-commerce behaviour (Amazon, Waterstones, Booktopia all do this).
    /// </summary>
    [Authorize]
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
        // Shows the shipping address form, pre-filled from the user's profile
        public async Task<IActionResult> Address()
        {
            var cartVM = await _cartService.GetCartAsync();

            if (cartVM.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var user = await _userManager.GetUserAsync(User);

            // Pre-fill address from user profile (courtesy, not required)
            var vm = new CheckoutVM
            {
                ShippingName = user?.Name ?? user?.UserName ?? string.Empty,
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
            // Attach cart data (it's not part of the form POST)
            vm.Cart = await _cartService.GetCartAsync();

            if (vm.Cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
                return View(vm);

            // Store the address in TempData so Review page can display it
            // TempData survives exactly one redirect — perfect for this flow.
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
        // Shows the full order summary (items + address + totals) for final confirmation
        public async Task<IActionResult> Review()
        {
            // Restore address from TempData
            if (TempData["ShippingName"] == null)
                return RedirectToAction(nameof(Address));

            var cartVM = await _cartService.GetCartAsync();
            if (cartVM.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutVM
            {
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

            // Keep TempData alive for the PlaceOrder POST
            TempData.Keep();

            return View(vm);
        }

        // ── POST /Checkout/PlaceOrder ──────────────────────────────────────────
        // Atomically creates the order, decrements stock, and clears the cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            if (TempData["ShippingName"] == null)
                return RedirectToAction(nameof(Address));

            var vm = new CheckoutVM
            {
                ShippingName = TempData["ShippingName"]?.ToString() ?? string.Empty,
                ShippingStreetAddress = TempData["ShippingStreetAddress"]?.ToString() ?? string.Empty,
                ShippingCity = TempData["ShippingCity"]?.ToString() ?? string.Empty,
                ShippingState = TempData["ShippingState"]?.ToString(),
                ShippingPostalCode = TempData["ShippingPostalCode"]?.ToString() ?? string.Empty,
                ShippingCountry = TempData["ShippingCountry"]?.ToString() ?? string.Empty,
                ShippingPhoneNumber = TempData["ShippingPhoneNumber"]?.ToString(),
                CustomerNote = TempData["CustomerNote"]?.ToString()
            };

            var userId = _userManager.GetUserId(User)!;

            try
            {
                int orderId = await _orderService.PlaceOrderAsync(vm, userId);
                return RedirectToAction(nameof(Confirmation), new { id = orderId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["OrderError"] = ex.Message;
                TempData.Keep();
                return RedirectToAction(nameof(Review));
            }
        }

        // ── GET /Checkout/Confirmation/{id} ───────────────────────────────────
        // Displays the receipt after a successful order placement
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _orderService.GetOrderConfirmationAsync(id, userId);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
    }
}
