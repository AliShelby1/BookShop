using BookShop.Data;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthorController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Author
        public async Task<IActionResult> Index()
        {
            var authorList = await _db.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(authorList);
        }

        // GET: /Author/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                _db.Authors.Add(author);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Author '{author.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: /Author/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var author = await _db.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: /Author/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Author author)
        {
            if (ModelState.IsValid)
            {
                _db.Authors.Update(author);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Author '{author.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // POST: /Author/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null)
            {
                TempData["error"] = "Author not found.";
                return RedirectToAction(nameof(Index));
            }

            // Referential integrity check
            var bookCount = await _db.Books.CountAsync(b => b.AuthorId == id);
            if (bookCount > 0)
            {
                TempData["error"] = $"Cannot delete author '{author.Name}' because they currently have {bookCount} book(s) in the store. Please reassign or remove their books first.";
                return RedirectToAction(nameof(Index));
            }

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            TempData["success"] = $"Author '{author.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
