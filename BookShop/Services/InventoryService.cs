using BookShop.Data;
using BookShop.Models;
using BookShop.Models.Enums;
using BookShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryDashboardVM> GetDashboardAsync(string? filter = null, string? search = null)
        {
            var baseQuery = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .AsQueryable();

            // Calculate aggregate metrics across catalog
            var totalTitles = await _context.Books.CountAsync();
            var totalUnits = await _context.Books.SumAsync(b => (int)b.StockQuantity);
            var lowStockCount = await _context.Books.CountAsync(b => b.StockQuantity > 0 && b.StockQuantity <= 5);
            var outOfStockCount = await _context.Books.CountAsync(b => b.StockQuantity <= 0);

            var query = baseQuery;

            // Apply stock level filter tabs
            if (filter == "low")
            {
                query = query.Where(b => b.StockQuantity > 0 && b.StockQuantity <= 5);
            }
            else if (filter == "out")
            {
                query = query.Where(b => b.StockQuantity <= 0);
            }

            // Apply search string (title, author, ISBN)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(term) ||
                    (b.TitleAr != null && b.TitleAr.ToLower().Contains(term)) ||
                    (b.ISBN != null && b.ISBN.ToLower().Contains(term)) ||
                    (b.Author != null && b.Author.Name.ToLower().Contains(term))
                );
            }

            // Order with low/out of stock items first, then alphabetical
            var books = await query
                .OrderBy(b => b.StockQuantity)
                .ThenBy(b => b.Title)
                .Select(b => new InventoryBookItemVM
                {
                    BookId = b.Id,
                    Title = b.Title,
                    TitleAr = b.TitleAr,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    CategoryNameAr = b.Category != null ? b.Category.NameAr : null,
                    ISBN = b.ISBN,
                    Price = b.Price,
                    StockQuantity = b.StockQuantity,
                    CoverImageUrl = b.CoverImageUrl,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            return new InventoryDashboardVM
            {
                Books = books,
                TotalTitlesCount = totalTitles,
                TotalUnitsInStock = totalUnits,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                Filter = filter,
                SearchString = search
            };
        }

        public async Task<StockAdjustmentVM?> GetStockAdjustmentModelAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return null;

            return new StockAdjustmentVM
            {
                BookId = book.Id,
                BookTitle = book.Title,
                BookTitleAr = book.TitleAr,
                CoverImageUrl = book.CoverImageUrl,
                CurrentStock = book.StockQuantity,
                QuantityChange = 0,
                TransactionType = InventoryTransactionType.Purchase,
                Reason = string.Empty
            };
        }

        public async Task<(bool Success, string Message)> AdjustStockAsync(StockAdjustmentVM model, string createdBy)
        {
            if (model.QuantityChange == 0)
                return (false, "Quantity change cannot be zero.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books.FindAsync(model.BookId);
                if (book == null)
                    return (false, "Book not found.");

                int quantityBefore = book.StockQuantity;
                int newStock = quantityBefore + model.QuantityChange;

                if (newStock < 0)
                    return (false, $"Adjustment would cause stock to drop below zero (Current: {quantityBefore}, Requested change: {model.QuantityChange}).");

                // Update physical stock balance
                book.StockQuantity = newStock;
                _context.Books.Update(book);

                // Insert immutable audit ledger record
                var ledgerEntry = new InventoryTransaction
                {
                    BookId = book.Id,
                    QuantityChange = model.QuantityChange,
                    QuantityBefore = quantityBefore,
                    QuantityAfter = newStock,
                    TransactionType = model.TransactionType,
                    Reason = string.IsNullOrWhiteSpace(model.Reason) ? "Manual inventory adjustment" : model.Reason.Trim(),
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(ledgerEntry);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Successfully updated stock for '{book.Title}' from {quantityBefore} to {newStock}.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to adjust stock: {ex.Message}");
            }
        }

        public async Task<InventoryHistoryVM> GetHistoryAsync(int? bookId = null, InventoryTransactionType? type = null, string? search = null)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Book)
                .AsQueryable();

            string? selectedBookTitle = null;

            if (bookId.HasValue)
            {
                query = query.Where(t => t.BookId == bookId.Value);
                var b = await _context.Books.FindAsync(bookId.Value);
                selectedBookTitle = b?.Title;
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.TransactionType == type.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(t =>
                    t.Book!.Title.ToLower().Contains(term) ||
                    (t.Book.TitleAr != null && t.Book.TitleAr.ToLower().Contains(term)) ||
                    (t.Reason != null && t.Reason.ToLower().Contains(term)) ||
                    t.CreatedBy.ToLower().Contains(term)
                );
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new InventoryTransactionItemVM
                {
                    Id = t.Id,
                    BookId = t.BookId,
                    BookTitle = t.Book != null ? t.Book.Title : "Unknown",
                    BookTitleAr = t.Book != null ? t.Book.TitleAr : null,
                    QuantityChange = t.QuantityChange,
                    QuantityBefore = t.QuantityBefore,
                    QuantityAfter = t.QuantityAfter,
                    TransactionType = t.TransactionType,
                    Reason = t.Reason,
                    CreatedBy = t.CreatedBy,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new InventoryHistoryVM
            {
                Transactions = items,
                SelectedBookId = bookId,
                SelectedBookTitle = selectedBookTitle,
                SelectedType = type,
                SearchQuery = search,
                TotalTransactionsCount = totalCount
            };
        }

        public async Task RecordTransactionAsync(int bookId, int quantityChange, int quantityBefore, int quantityAfter, InventoryTransactionType type, string reason, string createdBy)
        {
            var entry = new InventoryTransaction
            {
                BookId = bookId,
                QuantityChange = quantityChange,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                TransactionType = type,
                Reason = reason,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}
