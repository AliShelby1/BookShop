using BookShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILanguageService _lang;

        public WishlistController(IWishlistService wishlistService, ILanguageService lang)
        {
            _wishlistService = wishlistService;
            _lang = lang;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var wishlistVM = await _wishlistService.GetWishlistAsync();
            return View(wishlistVM);
        }

        // POST: /Wishlist/Toggle
        // Used by AJAX for one-click heart button toggling on cards and details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle([FromForm] int bookId)
        {
            var result = await _wishlistService.ToggleWishlistAsync(bookId);
            return Json(result);
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove([FromForm] int bookId, [FromForm] string? returnUrl = null)
        {
            var result = await _wishlistService.RemoveFromWishlistAsync(bookId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(result);
            }

            TempData["Success"] = result.Message;
            return LocalRedirect(returnUrl ?? "/Wishlist");
        }

        // POST: /Wishlist/MoveToBag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToBag([FromForm] int bookId, [FromForm] string? returnUrl = null)
        {
            var result = await _wishlistService.MoveToBagAsync(bookId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(result);
            }

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return LocalRedirect(returnUrl ?? "/Wishlist");
        }

        // GET: /Wishlist/GetCount
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await _wishlistService.GetWishlistItemCountAsync();
            return Json(new { count });
        }
    }
}
