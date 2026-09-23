using BookShop.Data;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Services
{
    public class WishlistService : IWishlistService
    {
        private const string GuestWishlistCookieName = "BookShop_WishlistSessionId";
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        private readonly ILanguageService _lang;

        public WishlistService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            ICartService cartService,
            ILanguageService lang)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _cartService = cartService;
            _lang = lang;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is unavailable.");

        public async Task<WishlistVM> GetWishlistAsync()
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();

            var vm = new WishlistVM
            {
                Items = wishlist.Items
                    .Where(i => i.Book != null)
                    .OrderByDescending(i => i.AddedAt)
                    .Select(i => new WishlistItemVM
                    {
                        WishlistItemId = i.Id,
                        BookId = i.BookId,
                        Title = i.Book!.Title,
                        TitleAr = i.Book.TitleAr,
                        AuthorName = i.Book.Author?.Name,
                        CategoryName = i.Book.Category?.Name,
                        CategoryNameAr = i.Book.Category?.NameAr,
                        CoverImageUrl = i.Book.CoverImageUrl,
                        OriginalPrice = i.Book.Price,
                        UnitPrice = i.Book.EffectivePrice,
                        DiscountPercentage = i.Book.DiscountPercentage,
                        StockQuantity = i.Book.StockQuantity,
                        IsActive = i.Book.IsActive,
                        AddedAt = i.AddedAt
                    })
                    .ToList()
            };

            return vm;
        }

        public async Task<WishlistOperationResult> ToggleWishlistAsync(int bookId)
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();
            var existingItem = wishlist.Items.FirstOrDefault(i => i.BookId == bookId);

            if (existingItem != null)
            {
                // Remove from wishlist
                _context.WishlistItems.Remove(existingItem);
                wishlist.Items.Remove(existingItem);
                wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _cachedWishlistBookIds = null;

                var count = wishlist.Items.Count;
                var msg = _lang.IsRtl ? "تمت إزالة الكتاب من قائمة القراءة" : "Removed from your reading list";
                return WishlistOperationResult.Ok(msg, inWishlist: false, wishlistCount: count);
            }
            else
            {
                // Add to wishlist
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);
                if (book == null)
                {
                    var notFoundMsg = _lang.IsRtl ? "هذا الإصدار غير متوفر حالياً" : "This edition is currently unavailable";
                    return WishlistOperationResult.Fail(notFoundMsg);
                }

                var newItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    BookId = bookId,
                    AddedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(newItem);
                wishlist.Items.Add(newItem);
                wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _cachedWishlistBookIds = null;

                var count = wishlist.Items.Count;
                var msg = _lang.IsRtl ? "تمت إضافة الكتاب إلى قائمة القراءة" : "Saved to your reading list";
                return WishlistOperationResult.Ok(msg, inWishlist: true, wishlistCount: count);
            }
        }

        public async Task<WishlistOperationResult> AddToWishlistAsync(int bookId)
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();
            var existingItem = wishlist.Items.FirstOrDefault(i => i.BookId == bookId);

            if (existingItem != null)
            {
                var msgAlready = _lang.IsRtl ? "هذا الكتاب موجود بالفعل في قائمتك" : "This volume is already in your reading list";
                return WishlistOperationResult.Ok(msgAlready, inWishlist: true, wishlistCount: wishlist.Items.Count);
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);
            if (book == null)
            {
                var notFoundMsg = _lang.IsRtl ? "هذا الإصدار غير متوفر حالياً" : "This edition is currently unavailable";
                return WishlistOperationResult.Fail(notFoundMsg);
            }

            var newItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            };

            _context.WishlistItems.Add(newItem);
            wishlist.Items.Add(newItem);
            wishlist.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cachedWishlistBookIds = null;

            var count = wishlist.Items.Count;
            var msg = _lang.IsRtl ? "تمت إضافة الكتاب إلى قائمة القراءة" : "Saved to your reading list";
            return WishlistOperationResult.Ok(msg, inWishlist: true, wishlistCount: count);
        }

        public async Task<WishlistOperationResult> RemoveFromWishlistAsync(int bookId)
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();
            var existingItem = wishlist.Items.FirstOrDefault(i => i.BookId == bookId);

            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                wishlist.Items.Remove(existingItem);
                wishlist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _cachedWishlistBookIds = null;
            }

            var count = wishlist.Items.Count;
            var msg = _lang.IsRtl ? "تمت إزالة الكتاب من قائمة القراءة" : "Removed from your reading list";
            return WishlistOperationResult.Ok(msg, inWishlist: false, wishlistCount: count);
        }

        public async Task<WishlistOperationResult> MoveToBagAsync(int bookId)
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();
            var wishlistItem = wishlist.Items.FirstOrDefault(i => i.BookId == bookId);

            if (wishlistItem == null)
            {
                var notInListMsg = _lang.IsRtl ? "هذا الكتاب ليس في قائمتك" : "Volume is not in your reading list";
                return WishlistOperationResult.Fail(notInListMsg);
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);
            if (book == null || book.StockQuantity <= 0)
            {
                var outOfStockMsg = _lang.IsRtl ? "نعتذر، هذا الإصدار نفد من المخزون حالياً" : "This volume is currently out of stock";
                return WishlistOperationResult.Fail(outOfStockMsg);
            }

            // 1. Add to Bag (Cart)
            var cartResult = await _cartService.AddToCartAsync(bookId, 1);
            if (!cartResult.Success)
            {
                return WishlistOperationResult.Fail(cartResult.Message);
            }

            // 2. Remove from Wishlist
            _context.WishlistItems.Remove(wishlistItem);
            wishlist.Items.Remove(wishlistItem);
            wishlist.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cachedWishlistBookIds = null;

            var wishlistCount = wishlist.Items.Count;
            var cartCount = await _cartService.GetCartItemCountAsync();
            var successMsg = _lang.IsRtl ? "تم نقل الكتاب إلى حقيبة القراءة بنجاح" : "Volume transferred to your reading bag";

            return WishlistOperationResult.Ok(successMsg, inWishlist: false, wishlistCount: wishlistCount, cartCount: cartCount);
        }

        private HashSet<int>? _cachedWishlistBookIds;

        public async Task<HashSet<int>> GetUserWishlistBookIdsAsync()
        {
            if (_cachedWishlistBookIds != null)
            {
                return _cachedWishlistBookIds;
            }

            var wishlist = await GetOrCreateWishlistInternalAsync();
            _cachedWishlistBookIds = new HashSet<int>(wishlist.Items.Select(i => i.BookId));
            return _cachedWishlistBookIds;
        }

        public async Task<int> GetWishlistItemCountAsync()
        {
            var wishlist = await GetOrCreateWishlistInternalAsync();
            return wishlist.Items.Count;
        }

        public async Task MergeGuestWishlistAsync(string userId)
        {
            var guestSessionId = HttpContext.Request.Cookies[GuestWishlistCookieName];
            if (string.IsNullOrEmpty(guestSessionId)) return;

            var guestWishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.SessionWishlistId == guestSessionId);

            if (guestWishlist == null || !guestWishlist.Items.Any())
            {
                HttpContext.Response.Cookies.Delete(GuestWishlistCookieName);
                return;
            }

            var userWishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.ApplicationUserId == userId);

            if (userWishlist == null)
            {
                userWishlist = new Wishlist
                {
                    ApplicationUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wishlists.Add(userWishlist);
                await _context.SaveChangesAsync();
            }

            foreach (var guestItem in guestWishlist.Items.ToList())
            {
                // Duplicate prevention: only add if the user does not already have it
                bool alreadyHasBook = userWishlist.Items.Any(i => i.BookId == guestItem.BookId);
                if (!alreadyHasBook)
                {
                    guestItem.WishlistId = userWishlist.Id;
                    userWishlist.Items.Add(guestItem);
                }
            }

            _context.Wishlists.Remove(guestWishlist);
            await _context.SaveChangesAsync();

            HttpContext.Response.Cookies.Delete(GuestWishlistCookieName);
        }

        private async Task<Wishlist> GetOrCreateWishlistInternalAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                // If a guest cookie exists from before login, merge it
                await MergeGuestWishlistAsync(user.Id);

                var userWishlist = await _context.Wishlists
                    .Include(w => w.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Author)
                    .Include(w => w.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Category)
                    .FirstOrDefaultAsync(w => w.ApplicationUserId == user.Id);

                if (userWishlist == null)
                {
                    userWishlist = new Wishlist
                    {
                        ApplicationUserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Wishlists.Add(userWishlist);
                    await _context.SaveChangesAsync();
                }

                return userWishlist;
            }

            // Guest user: identify via secure browser cookie
            var guestSessionId = HttpContext.Request.Cookies[GuestWishlistCookieName];

            if (string.IsNullOrEmpty(guestSessionId))
            {
                guestSessionId = Guid.NewGuid().ToString("N");
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };
                HttpContext.Response.Cookies.Append(GuestWishlistCookieName, guestSessionId, cookieOptions);
            }

            var guestWishlist = await _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Book)
                        .ThenInclude(b => b!.Author)
                .Include(w => w.Items)
                    .ThenInclude(i => i.Book)
                        .ThenInclude(b => b!.Category)
                .FirstOrDefaultAsync(w => w.SessionWishlistId == guestSessionId);

            if (guestWishlist == null)
            {
                guestWishlist = new Wishlist
                {
                    SessionWishlistId = guestSessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wishlists.Add(guestWishlist);
                await _context.SaveChangesAsync();
            }

            return guestWishlist;
        }
    }
}
