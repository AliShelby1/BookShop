using BookShop.Data;
using BookShop.Models;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<OrderHeader> PlaceOrderAsync(CheckoutVM checkout, string? userId, string? sessionCartId = null)
        {
            // ─────────────────────────────────────────────────────────────────────
            // STEP 1: Load the current cart with all book data
            // ─────────────────────────────────────────────────────────────────────
            var cartVM = await _cartService.GetCartAsync();

            if (cartVM.Items.Count == 0)
                throw new InvalidOperationException("Cannot place an order with an empty cart.");

            // ─────────────────────────────────────────────────────────────────────
            // STEP 2: Use an EF Core Transaction to ensure atomicity.
            // ─────────────────────────────────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ─────────────────────────────────────────────────────────────────
                // STEP 3: Create the OrderHeader (master record)
                // If userId is null, this is saved as a Guest Order.
                // ─────────────────────────────────────────────────────────────────
                var orderHeader = new OrderHeader
                {
                    ApplicationUserId = userId, // null for guest, non-null for registered user
                    CustomerEmail = checkout.CustomerEmail.Trim(),
                    OrderGuid = Guid.NewGuid(),
                    SessionCartId = sessionCartId,
                    ShippingName = checkout.ShippingName,
                    ShippingStreetAddress = checkout.ShippingStreetAddress,
                    ShippingCity = checkout.ShippingCity,
                    ShippingState = checkout.ShippingState,
                    ShippingPostalCode = checkout.ShippingPostalCode,
                    ShippingCountry = checkout.ShippingCountry,
                    ShippingPhoneNumber = checkout.ShippingPhoneNumber,
                    CustomerNote = checkout.CustomerNote,
                    Subtotal = cartVM.Subtotal,
                    ShippingFee = cartVM.ShippingFee,
                    TotalDiscountSaved = cartVM.TotalDiscountSaved,
                    OrderTotal = cartVM.Total,
                    OrderStatus = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Paid, // Simulated payment
                    OrderDate = DateTime.UtcNow
                };

                _context.OrderHeaders.Add(orderHeader);
                await _context.SaveChangesAsync(); // Generates OrderHeader.Id

                // ─────────────────────────────────────────────────────────────────
                // STEP 4: Create OrderDetail rows (price + title snapshots)
                // ─────────────────────────────────────────────────────────────────
                foreach (var item in cartVM.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderHeaderId = orderHeader.Id,
                        BookId = item.BookId,
                        BookTitle = item.Title,           // SNAPSHOT
                        BookTitleAr = item.TitleAr,       // SNAPSHOT
                        AuthorName = item.AuthorName,     // SNAPSHOT
                        UnitPrice = item.UnitPrice,       // SNAPSHOT (effective discounted price)
                        OriginalPrice = item.OriginalPrice, // SNAPSHOT (for discount display)
                        Quantity = item.Quantity
                    };
                    _context.OrderDetails.Add(detail);

                    // ─────────────────────────────────────────────────────────────
                    // STEP 5: Decrement stock quantity for this book
                    // ─────────────────────────────────────────────────────────────
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null || !book.IsActive)
                        throw new InvalidOperationException($"Book '{item.Title}' is no longer available.");
                    if (book.StockQuantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for '{item.Title}'. Only {book.StockQuantity} left.");

                    book.StockQuantity -= item.Quantity;
                    _context.Books.Update(book);
                }

                await _context.SaveChangesAsync();

                // ─────────────────────────────────────────────────────────────────
                // STEP 6: Clear the cart (order is placed, bag is now empty)
                // ─────────────────────────────────────────────────────────────────
                await _cartService.ClearCartAsync();

                // ─────────────────────────────────────────────────────────────────
                // STEP 7: Commit transaction
                // ─────────────────────────────────────────────────────────────────
                await transaction.CommitAsync();

                return orderHeader;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderConfirmationVM?> GetOrderConfirmationAsync(int orderId, string? userId, Guid? orderGuid = null)
        {
            var order = await _context.OrderHeaders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // Security authorization check:
            // 1. Is the current user the logged-in owner of this order?
            bool isOwner = !string.IsNullOrEmpty(userId) && order.ApplicationUserId == userId;

            // 2. Is the current user a staff/admin member?
            bool isStaff = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users.FindAsync(userId);
                isStaff = user != null; // Refined in Phase 7 with role check
            }

            // 3. For guest orders: does the request provide the matching OrderGuid token?
            bool isValidGuestAccess = orderGuid.HasValue && order.OrderGuid == orderGuid.Value;

            if (!isOwner && !isStaff && !isValidGuestAccess)
                return null;

            return new OrderConfirmationVM
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                OrderGuid = order.OrderGuid,
                CustomerEmail = order.CustomerEmail,
                IsGuestOrder = string.IsNullOrEmpty(order.ApplicationUserId),
                ShippingName = order.ShippingName,
                ShippingStreetAddress = order.ShippingStreetAddress,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                ShippingPhoneNumber = order.ShippingPhoneNumber,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                TotalDiscountSaved = order.TotalDiscountSaved,
                OrderTotal = order.OrderTotal,
                Items = order.OrderDetails.Select(d => new OrderDetailVM
                {
                    BookId = d.BookId,
                    BookTitle = d.BookTitle,
                    BookTitleAr = d.BookTitleAr,
                    AuthorName = d.AuthorName,
                    CoverImageUrl = d.Book?.CoverImageUrl,
                    UnitPrice = d.UnitPrice,
                    OriginalPrice = d.OriginalPrice,
                    Quantity = d.Quantity
                }).ToList()
            };
        }

        public async Task<List<OrderHeader>> GetOrdersByUserAsync(string userId)
        {
            return await _context.OrderHeaders
                .Where(o => o.ApplicationUserId == userId)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<OrderHeader>> GetAllOrdersAsync()
        {
            return await _context.OrderHeaders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}
