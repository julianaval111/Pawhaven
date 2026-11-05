using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, string? status)
        {
            var bookings = _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.GroomingService)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bookings = bookings.Where(b =>
                    (b.Owner != null && b.Owner.FullName.Contains(searchString)) ||
                    (b.Pet != null && b.Pet.PetName.Contains(searchString)) ||
                    (b.GroomingService != null &&
                     b.GroomingService.ServiceName.Contains(searchString)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bookings = bookings.Where(b => b.Status == status);
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            return View(await bookings
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync());
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.GroomingService)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            ViewBag.Statuses = new SelectList(
                new[] { "Pending", "Approved", "Rejected", "Cancelled" },
                booking.Status);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            var allowedStatuses = new[]
            {
                "Pending", "Approved", "Rejected", "Cancelled"
            };

            if (!allowedStatuses.Contains(status))
            {
                ModelState.AddModelError("Status", "Invalid booking status.");

                ViewBag.Statuses = new SelectList(allowedStatuses, booking.Status);

                return View(booking);
            }

            booking.Status = status;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking status updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}