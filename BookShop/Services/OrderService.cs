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
            var cartVM = await _cartService.GetCartAsync();

            if (cartVM.Items.Count == 0)
                throw new InvalidOperationException("Cannot place an order with an empty cart.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
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
                    PaymentStatus = PaymentStatus.Paid,
                    OrderDate = DateTime.UtcNow
                };

                _context.OrderHeaders.Add(orderHeader);
                await _context.SaveChangesAsync();

                foreach (var item in cartVM.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderHeaderId = orderHeader.Id,
                        BookId = item.BookId,
                        BookTitle = item.Title,
                        BookTitleAr = item.TitleAr,
                        AuthorName = item.AuthorName,
                        UnitPrice = item.UnitPrice,
                        OriginalPrice = item.OriginalPrice,
                        Quantity = item.Quantity
                    };
                    _context.OrderDetails.Add(detail);

                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null || !book.IsActive)
                        throw new InvalidOperationException($"Book '{item.Title}' is no longer available.");
                    if (book.StockQuantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for '{item.Title}'. Only {book.StockQuantity} left.");

                    book.StockQuantity -= item.Quantity;
                    _context.Books.Update(book);
                }

                await _context.SaveChangesAsync();
                await _cartService.ClearCartAsync();
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

            bool isOwner = !string.IsNullOrEmpty(userId) && order.ApplicationUserId == userId;
            bool isStaff = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users.FindAsync(userId);
                isStaff = user != null;
            }
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

        public async Task<OrderListVM> GetOrdersListAsync(string? userId, bool isStaff, OrderStatus? status = null, string? search = null)
        {
            var baseQuery = _context.OrderHeaders
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // Customer isolation check: regular users only see their own orders
            if (!isStaff)
            {
                if (string.IsNullOrEmpty(userId))
                    return new OrderListVM(); // Anonymous user has no order dashboard

                baseQuery = baseQuery.Where(o => o.ApplicationUserId == userId);
            }

            // Calculate status counts based on customer or staff scope
            var totalCount = await baseQuery.CountAsync();
            var pendingCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Pending);
            var confirmedCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Confirmed);
            var preparingCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Preparing);
            var shippedCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Shipped);
            var deliveredCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Delivered);
            var cancelledCount = await baseQuery.CountAsync(o => o.OrderStatus == OrderStatus.Cancelled);

            var filteredQuery = baseQuery;

            // Apply Status Filter tab
            if (status.HasValue)
            {
                filteredQuery = filteredQuery.Where(o => o.OrderStatus == status.Value);
            }

            // Apply Search Filter (Order number, customer name, email, phone)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                // Check if term matches numeric order id (e.g. "1" or "BSH-000001")
                int parsedId = 0;
                if (term.StartsWith("bsh-") && int.TryParse(term.Replace("bsh-", "").TrimStart('0'), out var idFromCode))
                {
                    parsedId = idFromCode;
                }
                else if (int.TryParse(term, out var directId))
                {
                    parsedId = directId;
                }

                filteredQuery = filteredQuery.Where(o =>
                    (parsedId > 0 && o.Id == parsedId) ||
                    o.CustomerEmail.ToLower().Contains(term) ||
                    o.ShippingName.ToLower().Contains(term) ||
                    (o.ShippingPhoneNumber != null && o.ShippingPhoneNumber.Contains(term))
                );
            }

            var orders = await filteredQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return new OrderListVM
            {
                Orders = orders,
                StatusFilter = status,
                SearchString = search,
                IsAdminOrStaff = isStaff,
                TotalCount = totalCount,
                PendingCount = pendingCount,
                ConfirmedCount = confirmedCount,
                PreparingCount = preparingCount,
                ShippedCount = shippedCount,
                DeliveredCount = deliveredCount,
                CancelledCount = cancelledCount
            };
        }

        public async Task<OrderHeader?> GetOrderDetailsAsync(int orderId, string? userId, bool isStaff)
        {
            var order = await _context.OrderHeaders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // IDOR Protection: regular customer can only view their own order
            if (!isStaff && order.ApplicationUserId != userId)
                return null;

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order == null) return false;

            if (newStatus == OrderStatus.Cancelled)
            {
                return await CancelOrderAsync(orderId, null, isStaff: true);
            }

            order.OrderStatus = newStatus;

            if (newStatus == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                order.ShippedDate = DateTime.UtcNow;

            if (newStatus == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                order.DeliveredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string? userId, bool isStaff)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.OrderHeaders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                // Ownership check
                if (!isStaff && order.ApplicationUserId != userId)
                    return false;

                // Cancellation business rules:
                // Customer can only cancel if Pending or Confirmed
                if (!isStaff && order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Confirmed)
                    return false;

                // Staff cannot cancel already Delivered or Cancelled orders
                if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Cancelled)
                    return false;

                // ── Atomically Restock Books ─────────────────────────────────
                foreach (var item in order.OrderDetails)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book != null)
                    {
                        book.StockQuantity += item.Quantity;
                        _context.Books.Update(book);
                    }
                }

                // Update Status
                order.OrderStatus = OrderStatus.Cancelled;
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
