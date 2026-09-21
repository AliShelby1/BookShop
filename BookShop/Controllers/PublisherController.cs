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
    public class PublisherController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PublisherController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Publisher
        public async Task<IActionResult> Index()
        {
            var publisherList = await _db.Publishers
                .Include(p => p.Books)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(publisherList);
        }

        // GET: /Publisher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Publisher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                _db.Publishers.Add(publisher);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Publisher '{publisher.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // GET: /Publisher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var publisher = await _db.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: /Publisher/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                _db.Publishers.Update(publisher);
                await _db.SaveChangesAsync();
                TempData["success"] = $"Publisher '{publisher.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // POST: /Publisher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var publisher = await _db.Publishers.FindAsync(id);
            if (publisher == null)
            {
                TempData["error"] = "Publisher not found.";
                return RedirectToAction(nameof(Index));
            }

            _db.Publishers.Remove(publisher);
            await _db.SaveChangesAsync();
            TempData["success"] = $"Publisher '{publisher.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
