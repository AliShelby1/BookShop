using BookShop.Data;
using BookShop.Models.ViewModels;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /User
        public async Task<IActionResult> Index()
        {
            var userList = await _db.Users.ToListAsync();
            var userRoles = await _db.UserRoles.ToListAsync();
            var roles = await _db.Roles.ToListAsync();

            var userVMList = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var userRole = userRoles.FirstOrDefault(u => u.UserId == user.Id);
                string roleName = "None";
                if (userRole != null)
                {
                    roleName = roles.FirstOrDefault(u => u.Id == userRole.RoleId)?.Name ?? "None";
                }

                bool isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

                userVMList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roleName,
                    IsLocked = isLocked,
                    LockoutEnd = user.LockoutEnd,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode
                });
            }

            return View(userVMList);
        }

        // POST: /User/LockUnlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["error"] = "You cannot lock your own administrator account!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Currently locked -> unlock
                user.LockoutEnd = DateTimeOffset.UtcNow;
                TempData["success"] = $"Account for {user.Email} has been unlocked successfully.";
            }
            else
            {
                // Currently unlocked -> lock for 1000 years
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1000);
                TempData["success"] = $"Account for {user.Email} has been locked.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/RoleManagement
        [HttpGet]
        public async Task<IActionResult> RoleManagement(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault() ?? SD.Role_Customer;

            var vm = new RoleManagementVM
            {
                UserId = user.Id,
                UserName = user.Name ?? user.UserName,
                Email = user.Email,
                CurrentRole = currentRole,
                SelectedRole = currentRole,
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };

            return View(vm);
        }

        // POST: /User/RoleManagement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleManagement(RoleManagementVM vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);
            var oldRoles = await _userManager.GetRolesAsync(user);
            var oldRole = oldRoles.FirstOrDefault();

            // Safety safeguard: Prevent current logged in admin from removing admin role from themselves
            if (user.Id == currentUserId && oldRole == SD.Role_Admin && vm.SelectedRole != SD.Role_Admin)
            {
                TempData["error"] = "You cannot remove the Admin role from your own account!";
                return RedirectToAction(nameof(Index));
            }

            if (oldRole != vm.SelectedRole)
            {
                if (!string.IsNullOrEmpty(oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRole);
                }
                await _userManager.AddToRoleAsync(user, vm.SelectedRole);
                TempData["success"] = $"Role for {user.Email} successfully changed to {vm.SelectedRole}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /User/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["error"] = "You cannot delete your own administrator account!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["success"] = $"User {user.Email} has been deleted successfully.";
            }
            else
            {
                TempData["error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
