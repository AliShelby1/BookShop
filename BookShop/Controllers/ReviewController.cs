using System.Security.Claims;
using System.Threading.Tasks;
using BookShop.Models.ViewModels;
using BookShop.Services;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // ── POST: /Review/Create ───────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewFormVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required review fields (rating and comment).";
                return RedirectToAction("Details", "Home", new { id = model.BookId });
            }

            var (success, message) = await _reviewService.SubmitReviewAsync(model, userId);
            if (success)
            {
                TempData["success"] = message;
            }
            else
            {
                TempData["error"] = message;
            }

            return Redirect($"/Home/Details/{model.BookId}#reviewsSection");
        }

        // ── GET: /Review/Edit/5 ────────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vm = await _reviewService.GetReviewForEditAsync(id, userId);
            if (vm == null)
            {
                TempData["error"] = "Review not found or you are not authorized to edit it.";
                return RedirectToAction("Index", "Home");
            }

            return View(vm);
        }

        // ── POST: /Review/Edit/5 ───────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewFormVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _reviewService.UpdateReviewAsync(id, model, userId);
            if (success)
            {
                TempData["success"] = message;
                return Redirect($"/Home/Details/{model.BookId}#reviewsSection");
            }

            TempData["error"] = message;
            return View(model);
        }

        // ── POST: /Review/Delete/5 ─────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            bool isStaff = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);
            var (success, message) = await _reviewService.DeleteReviewAsync(id, userId, isStaff);

            if (success)
            {
                TempData["success"] = message;
            }
            else
            {
                TempData["error"] = message;
            }

            return Redirect($"/Home/Details/{bookId}#reviewsSection");
        }

        // ── GET: /Review/Moderation ────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        public async Task<IActionResult> Moderation(bool? approved, string? search)
        {
            var vm = await _reviewService.GetReviewsForModerationAsync(approved, search);
            return View(vm);
        }

        // ── POST: /Review/ToggleApproval/5 ─────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id, string? returnUrl)
        {
            var success = await _reviewService.ToggleApprovalAsync(id);
            if (success)
            {
                TempData["success"] = "Review visibility updated successfully.";
            }
            else
            {
                TempData["error"] = "Failed to update review status.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Moderation));
        }

        // ── POST: /Review/AdminDelete/5 ────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDelete(int id, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (success, message) = await _reviewService.DeleteReviewAsync(id, userId, isStaff: true);

            if (success)
            {
                TempData["success"] = message;
            }
            else
            {
                TempData["error"] = message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Moderation));
        }
    }
}
