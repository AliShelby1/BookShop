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

        public async Task<int> PlaceOrderAsync(CheckoutVM checkout, string userId)
        {
            // ─────────────────────────────────────────────────────────────────────
            // STEP 1: Load the current cart with all book data
            // ─────────────────────────────────────────────────────────────────────
            var cartVM = await _cartService.GetCartAsync();

            if (cartVM.Items.Count == 0)
                throw new InvalidOperationException("Cannot place an order with an empty cart.");

            // ─────────────────────────────────────────────────────────────────────
            // STEP 2: Use an EF Core Transaction to ensure atomicity.
            //
            // 📚 LEARNING NOTE — Database Transactions:
            //
            // A transaction groups multiple database operations into a single
            // "all-or-nothing" unit. If ANY step fails (e.g., a book goes
            // out of stock mid-placement), ALL changes are rolled back and
            // the database is left in a clean, consistent state.
            //
            // Without a transaction, a crash between Step 3 and Step 4 could
            // create an order but leave the cart full and stock un-decremented.
            // ─────────────────────────────────────────────────────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ─────────────────────────────────────────────────────────────────
                // STEP 3: Create the OrderHeader (master record)
                // ─────────────────────────────────────────────────────────────────
                var orderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
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
                    PaymentStatus = PaymentStatus.Paid, // Simulated: mark as paid immediately
                    OrderDate = DateTime.UtcNow
                };

                _context.OrderHeaders.Add(orderHeader);
                await _context.SaveChangesAsync(); // Generates the OrderHeader.Id

                // ─────────────────────────────────────────────────────────────────
                // STEP 4: Create OrderDetail rows (price + title snapshots)
                //
                // This is the CRITICAL price immutability step.
                // We copy UnitPrice and BookTitle from the CURRENT cart item —
                // not a reference to Book.Price — so future price changes
                // CANNOT alter historical order receipts.
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
                        UnitPrice = item.UnitPrice,       // SNAPSHOT (effective price)
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
                // STEP 7: Commit the entire transaction
                // ─────────────────────────────────────────────────────────────────
                await transaction.CommitAsync();

                return orderHeader.Id;
            }
            catch
            {
                // If anything fails, roll back ALL changes — nothing is persisted
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderConfirmationVM?> GetOrderConfirmationAsync(int orderId, string userId)
        {
            var order = await _context.OrderHeaders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // Security: only the owning customer or staff can view the order
            var user = await _context.Users.FindAsync(userId);
            bool isStaff = user != null; // In Phase 7 we'll add role checks here

            if (order.ApplicationUserId != userId && !isStaff)
                return null;

            return new OrderConfirmationVM
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
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
