using BookShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService _cartService)
        {
            this._cartService = _cartService;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1, string? returnUrl = null)
        {
            var result = await _cartService.AddToCartAsync(bookId, quantity);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    count = result.TotalItemsCount
                });
            }

            if (result.Success)
            {
                TempData["success"] = result.Message;
            }
            else
            {
                TempData["error"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var result = await _cartService.UpdateQuantityAsync(cartItemId, quantity);

            if (IsAjaxRequest())
            {
                var cart = await _cartService.GetCartAsync();
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    count = result.TotalItemsCount,
                    cart = new
                    {
                        subtotal = $"${cart.Subtotal:0.00}",
                        totalDiscountSaved = $"${cart.TotalDiscountSaved:0.00}",
                        shippingFee = cart.ShippingFee == 0 ? "Complimentary" : $"${cart.ShippingFee:0.00}",
                        isFreeShipping = cart.IsFreeShipping,
                        amountNeededForFreeShipping = $"${cart.AmountNeededForFreeShipping:0.00}",
                        freeShippingProgressPercentage = cart.FreeShippingProgressPercentage,
                        total = $"${cart.Total:0.00}"
                    }
                });
            }

            if (result.Success)
            {
                TempData["success"] = result.Message;
            }
            else
            {
                TempData["error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var result = await _cartService.RemoveItemAsync(cartItemId);
            TempData["success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync();
            TempData["success"] = "Your reading bag has been cleared.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/GetCount
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await _cartService.GetCartItemCountAsync();
            return Json(new { count });
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers.Accept.ToString().Contains("application/json");
        }
    }
}
