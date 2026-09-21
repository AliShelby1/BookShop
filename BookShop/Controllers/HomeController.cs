using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Data;
using BookShop.Models;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /
        // GET: /
        public async Task<IActionResult> Index()
        {
            // 1. Fetch Admin-curated Volume of the Month (or fallback to first active book)
            var volumeOfTheMonth = await _db.Books
                .Where(b => b.IsActive && b.IsVolumeOfTheMonth)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .FirstOrDefaultAsync();

            if (volumeOfTheMonth == null)
            {
                volumeOfTheMonth = await _db.Books
                    .Where(b => b.IsActive)
                    .Include(b => b.Category)
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync();
            }

            // 2. Fetch Admin-curated Featured Books (or fallback to discounted books)
            var featuredBooks = await _db.Books
                .Where(b => b.IsActive && b.IsFeatured)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Take(8)
                .ToListAsync();

            if (!featuredBooks.Any())
            {
                featuredBooks = await _db.Books
                    .Where(b => b.IsActive && b.DiscountPercentage.HasValue && b.DiscountPercentage > 0)
                    .Include(b => b.Category)
                    .Include(b => b.Author)
                    .Take(4)
                    .ToListAsync();
            }

            var homeVM = new HomeVM
            {
                VolumeOfTheMonth = volumeOfTheMonth,
                FeaturedBooks = featuredBooks,
                Categories = await _db.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync(),
                NewArrivals = await _db.Books
                    .Where(b => b.IsActive)
                    .Include(b => b.Category)
                    .Include(b => b.Author)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(8)
                    .ToListAsync(),
                FeaturedAuthors = await _db.Authors
                    .Take(4)
                    .ToListAsync()
            };

            return View(homeVM);
        }

        // GET: /Home/Shop
        public async Task<IActionResult> Shop(ShopVM model)
        {
            var query = _db.Books
                .Where(b => b.IsActive)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsQueryable();

            // 1. Text Search Filter (Title or ISBN or Author)
            if (!string.IsNullOrWhiteSpace(model.SearchString))
            {
                var term = model.SearchString.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(term) 
                                      || b.ISBN.ToLower().Contains(term)
                                      || b.Author.Name.ToLower().Contains(term));
            }

            // 2. Category Filter
            if (model.CategoryId.HasValue && model.CategoryId > 0)
            {
                query = query.Where(b => b.CategoryId == model.CategoryId.Value);
            }

            // 3. Author Filter
            if (model.AuthorId.HasValue && model.AuthorId > 0)
            {
                query = query.Where(b => b.AuthorId == model.AuthorId.Value);
            }

            // 4. Price Filters
            if (model.MinPrice.HasValue && model.MinPrice > 0)
            {
                query = query.Where(b => b.Price >= model.MinPrice.Value);
            }
            if (model.MaxPrice.HasValue && model.MaxPrice > 0)
            {
                query = query.Where(b => b.Price <= model.MaxPrice.Value);
            }

            // 5. In-Stock Filter
            if (model.InStockOnly)
            {
                query = query.Where(b => b.StockQuantity > 0);
            }

            // 6. Sorting
            query = model.SortBy switch
            {
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "title_asc" => query.OrderBy(b => b.Title),
                _ => query.OrderByDescending(b => b.CreatedAt) // default: newest
            };

            // Total count before pagination
            model.TotalItems = await query.CountAsync();

            // 7. Database-side Pagination
            if (model.CurrentPage < 1) model.CurrentPage = 1;
            if (model.PageSize < 1) model.PageSize = 8;

            model.Books = await query
                .Skip((model.CurrentPage - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            // Populate Filter Sidebar Reference Data
            model.Categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            model.Authors = await _db.Authors
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(model);
        }

        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var book = await _db.Books
                .Where(b => b.Id == id && b.IsActive)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            var relatedBooks = await _db.Books
                .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id && b.IsActive)
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Take(4)
                .ToListAsync();

            var vm = new CustomerBookDetailsVM
            {
                Book = book,
                RelatedBooks = relatedBooks,
                DefaultQuantity = 1
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (culture == "ar" || culture == "en")
            {
                Response.Cookies.Append(
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax }
                );
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
