using BookShop.Data;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Services
{
    public class CartService : ICartService
    {
        private const string GuestCartCookieName = "BookShop_CartSessionId";
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is unavailable.");

        public async Task<CartVM> GetCartAsync()
        {
            var cart = await GetOrCreateCartInternalAsync();
            var settings = await GetStoreSettingsAsync();

            var isArabic = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
            var banner = (isArabic && !string.IsNullOrEmpty(settings.CartAnnouncementBannerAr))
                ? settings.CartAnnouncementBannerAr
                : settings.CartAnnouncementBanner;

            var vm = new CartVM
            {
                FreeShippingThreshold = settings.FreeShippingThreshold,
                FlatShippingRate = settings.FlatShippingRate,
                CartAnnouncementBanner = banner,
                Items = cart.Items
                    .Where(i => i.Book != null)
                    .Select(i => new CartItemVM
                    {
                        CartItemId = i.Id,
                        BookId = i.BookId,
                        Title = i.Book!.Title,
                        AuthorName = i.Book.Author?.Name,
                        CategoryName = i.Book.Category?.Name,
                        CoverImageUrl = i.Book.CoverImageUrl,
                        OriginalPrice = i.Book.Price,
                        UnitPrice = i.Book.EffectivePrice,
                        DiscountPercentage = i.Book.DiscountPercentage,
                        Quantity = i.Quantity,
                        StockQuantity = i.Book.StockQuantity
                    })
                    .ToList()
            };

            return vm;
        }

        public async Task<CartOperationResult> AddToCartAsync(int bookId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return CartOperationResult.Fail("Quantity must be at least 1.");
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null || !book.IsActive)
            {
                return CartOperationResult.Fail("This volume is currently unavailable.");
            }

            if (book.StockQuantity <= 0)
            {
                return CartOperationResult.Fail($"'{book.Title}' is currently out of stock.");
            }

            var settings = await GetStoreSettingsAsync();
            var cart = await GetOrCreateCartInternalAsync();

            var cartItem = cart.Items.FirstOrDefault(i => i.BookId == bookId);
            int targetQuantity = (cartItem?.Quantity ?? 0) + quantity;

            if (targetQuantity > book.StockQuantity)
            {
                return CartOperationResult.Fail($"Only {book.StockQuantity} {(book.StockQuantity == 1 ? "copy is" : "copies are")} available in our shelves.");
            }

            if (targetQuantity > settings.MaxQuantityPerBook)
            {
                return CartOperationResult.Fail($"Orders are limited to {settings.MaxQuantityPerBook} copies per title.");
            }

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    BookId = bookId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity = targetQuantity;
                _context.CartItems.Update(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            int totalCount = cart.Items.Sum(i => i.Quantity);
            return CartOperationResult.Ok($"Added \"{book.Title}\" to your reading bag.", totalCount);
        }

        public async Task<CartOperationResult> UpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                return await RemoveItemAsync(cartItemId);
            }

            var cart = await GetOrCreateCartInternalAsync();
            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (cartItem == null)
            {
                return CartOperationResult.Fail("Selected item was not found in your bag.");
            }

            var book = await _context.Books.FindAsync(cartItem.BookId);
            if (book == null || !book.IsActive)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return CartOperationResult.Fail("This volume is no longer available.");
            }

            var settings = await GetStoreSettingsAsync();

            if (newQuantity > book.StockQuantity)
            {
                cartItem.Quantity = book.StockQuantity;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
                return CartOperationResult.Fail($"Only {book.StockQuantity} copies are available. Quantity set to maximum stock.");
            }

            if (newQuantity > settings.MaxQuantityPerBook)
            {
                cartItem.Quantity = settings.MaxQuantityPerBook;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
                return CartOperationResult.Fail($"Maximum {settings.MaxQuantityPerBook} copies allowed per order.");
            }

            cartItem.Quantity = newQuantity;
            cart.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            int totalCount = cart.Items.Sum(i => i.Quantity);
            return CartOperationResult.Ok("Quantity updated.", totalCount);
        }

        public async Task<CartOperationResult> RemoveItemAsync(int cartItemId)
        {
            var cart = await GetOrCreateCartInternalAsync();
            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                cart.Items.Remove(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            int totalCount = cart.Items.Sum(i => i.Quantity);
            return CartOperationResult.Ok("Item removed from your bag.", totalCount);
        }

        public async Task ClearCartAsync()
        {
            var cart = await GetOrCreateCartInternalAsync();
            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var cart = await GetOrCreateCartInternalAsync();
            return cart.Items.Sum(i => i.Quantity);
        }

        public async Task MergeGuestCartAsync(string userId)
        {
            var guestSessionId = HttpContext.Request.Cookies[GuestCartCookieName];
            if (string.IsNullOrEmpty(guestSessionId)) return;

            var guestCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionCartId == guestSessionId);

            if (guestCart == null || !guestCart.Items.Any())
            {
                HttpContext.Response.Cookies.Delete(GuestCartCookieName);
                return;
            }

            var userCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (userCart == null)
            {
                userCart = new Cart
                {
                    ApplicationUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
            }

            foreach (var guestItem in guestCart.Items.ToList())
            {
                var existingUserItem = userCart.Items.FirstOrDefault(i => i.BookId == guestItem.BookId);
                if (existingUserItem != null)
                {
                    existingUserItem.Quantity += guestItem.Quantity;
                    _context.CartItems.Update(existingUserItem);
                }
                else
                {
                    guestItem.CartId = userCart.Id;
                    userCart.Items.Add(guestItem);
                }
            }

            _context.Carts.Remove(guestCart);
            await _context.SaveChangesAsync();

            HttpContext.Response.Cookies.Delete(GuestCartCookieName);
        }

        public async Task<StoreSetting> GetStoreSettingsAsync()
        {
            var settings = await _context.StoreSettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (settings == null)
            {
                settings = new StoreSetting
                {
                    Id = 1,
                    FreeShippingThreshold = 50.00m,
                    FlatShippingRate = 4.99m,
                    MaxQuantityPerBook = 10,
                    CartAnnouncementBanner = "Complimentary literary bookmark & archival packaging on all orders over $50.",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.StoreSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task UpdateStoreSettingsAsync(StoreSetting settings)
        {
            var current = await GetStoreSettingsAsync();
            current.FreeShippingThreshold = settings.FreeShippingThreshold;
            current.FlatShippingRate = settings.FlatShippingRate;
            current.MaxQuantityPerBook = settings.MaxQuantityPerBook;
            current.CartAnnouncementBanner = settings.CartAnnouncementBanner;
            current.CartAnnouncementBannerAr = settings.CartAnnouncementBannerAr;
            current.DefaultLanguage = string.IsNullOrWhiteSpace(settings.DefaultLanguage) ? "en" : settings.DefaultLanguage;
            current.EnableLanguageSwitcher = settings.EnableLanguageSwitcher;
            current.UpdatedAt = DateTime.UtcNow;

            _context.StoreSettings.Update(current);
            await _context.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartInternalAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                // If a guest cookie exists, merge it into this user's cart
                await MergeGuestCartAsync(user.Id);

                var userCart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Author)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Category)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (userCart == null)
                {
                    userCart = new Cart
                    {
                        ApplicationUserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Carts.Add(userCart);
                    await _context.SaveChangesAsync();
                }

                return userCart;
            }
            else
            {
                string? guestId = HttpContext.Request.Cookies[GuestCartCookieName];
                if (string.IsNullOrWhiteSpace(guestId))
                {
                    guestId = Guid.NewGuid().ToString();
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    };
                    HttpContext.Response.Cookies.Append(GuestCartCookieName, guestId, cookieOptions);
                }

                var guestCart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Author)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Book)
                            .ThenInclude(b => b!.Category)
                    .FirstOrDefaultAsync(c => c.SessionCartId == guestId);

                if (guestCart == null)
                {
                    guestCart = new Cart
                    {
                        SessionCartId = guestId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Carts.Add(guestCart);
                    await _context.SaveChangesAsync();
                }

                return guestCart;
            }
        }
    }
}
