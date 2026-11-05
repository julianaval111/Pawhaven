using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                users = users.Where(u =>
                    u.FullName.Contains(searchString) ||
                    (u.Email != null && u.Email.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await users.OrderBy(u => u.FullName).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot delete your own Admin account.";
                return RedirectToAction(nameof(Index));
            }

            bool hasPets = await _context.Pets.AnyAsync(p => p.UserId == id);
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.UserId == id);

            if (hasPets || hasBookings)
            {
                TempData["Error"] =
                    "This account cannot be deleted because it has related records.";

                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded
                    ? "User deleted successfully."
                    : string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }
    }
}