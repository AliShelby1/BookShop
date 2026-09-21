using BookShop.Data;
using BookShop.Models;
using BookShop.Models.ViewModels;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Book
        public async Task<IActionResult> Index(string? searchString, int? categoryId)
        {
            var booksQuery = _db.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.Trim().ToLower();
                booksQuery = booksQuery.Where(b => b.Title.ToLower().Contains(term) || b.ISBN.ToLower().Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == categoryId
                })
                .ToListAsync();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;

            var books = await booksQuery.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return View(books);
        }

        // GET: /Book/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var book = await _db.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: /Book/Upsert?id=5 (or no id for Create)
        public async Task<IActionResult> Upsert(int? id)
        {
            var bookVM = new BookVM
            {
                CategoryList = await GetCategorySelectListAsync(),
                AuthorList = await GetAuthorSelectListAsync(),
                PublisherList = await GetPublisherSelectListAsync(),
                Book = new Book()
            };

            if (id == null || id == 0)
            {
                // Create mode
                return View(bookVM);
            }
            else
            {
                // Edit mode
                var bookFromDb = await _db.Books.FindAsync(id);
                if (bookFromDb == null)
                {
                    return NotFound();
                }
                bookVM.Book = bookFromDb;
                return View(bookVM);
            }
        }

        // POST: /Book/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(BookVM bookVM, IFormFile? file)
        {
            // Check for duplicate ISBN
            if (await _db.Books.AnyAsync(b => b.ISBN.ToLower() == bookVM.Book.ISBN.ToLower() && b.Id != bookVM.Book.Id))
            {
                ModelState.AddModelError("Book.ISBN", "A book with this ISBN already exists.");
            }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string uploadsFolder = Path.Combine(wwwRootPath, @"images\books");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string extension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Only image files (.jpg, .jpeg, .png, .webp) are allowed for cover images.");
                        bookVM.CategoryList = await GetCategorySelectListAsync();
                        bookVM.AuthorList = await GetAuthorSelectListAsync();
                        bookVM.PublisherList = await GetPublisherSelectListAsync();
                        return View(bookVM);
                    }

                    string fileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Delete old image file on disk if it was a local file and not an external URL
                    if (!string.IsNullOrEmpty(bookVM.Book.CoverImageUrl) && bookVM.Book.CoverImageUrl.StartsWith("/images/books/"))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, bookVM.Book.CoverImageUrl.TrimStart('/').Replace('/', '\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    bookVM.Book.CoverImageUrl = @"/images/books/" + fileName;
                }
                else if (bookVM.Book.Id > 0)
                {
                    // Preserves existing cover image when editing without uploading a new one
                    var existingBook = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookVM.Book.Id);
                    if (existingBook != null)
                    {
                        bookVM.Book.CoverImageUrl = existingBook.CoverImageUrl;
                    }
                }

                if (bookVM.Book.Id == 0)
                {
                    bookVM.Book.CreatedAt = DateTime.UtcNow;
                    _db.Books.Add(bookVM.Book);
                    TempData["success"] = $"Book '{bookVM.Book.Title}' created successfully.";
                }
                else
                {
                    bookVM.Book.UpdatedAt = DateTime.UtcNow;
                    _db.Books.Update(bookVM.Book);
                    TempData["success"] = $"Book '{bookVM.Book.Title}' updated successfully.";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Redisplay form if model state is invalid
            bookVM.CategoryList = await GetCategorySelectListAsync();
            bookVM.AuthorList = await GetAuthorSelectListAsync();
            bookVM.PublisherList = await GetPublisherSelectListAsync();
            return View(bookVM);
        }

        // POST: /Book/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                TempData["error"] = "Book not found.";
                return RedirectToAction(nameof(Index));
            }

            book.IsActive = !book.IsActive;
            book.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["success"] = $"Book '{book.Title}' is now {(book.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Book/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                TempData["error"] = "Book not found.";
                return RedirectToAction(nameof(Index));
            }

            // Clean up physical image file if local
            if (!string.IsNullOrEmpty(book.CoverImageUrl) && book.CoverImageUrl.StartsWith("/images/books/"))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImageUrl.TrimStart('/').Replace('/', '\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            TempData["success"] = $"Book '{book.Title}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper methods for SelectListItems
        private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetAuthorSelectListAsync()
        {
            return await _db.Authors
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetPublisherSelectListAsync()
        {
            return await _db.Publishers
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                })
                .ToListAsync();
        }
    }
}
