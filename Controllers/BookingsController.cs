
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Services;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleCalendarService _googleCalendarService;
        private readonly BookingAvailabilityService _availabilityService;

        public BookingsController(
            ApplicationDbContext context,
            GoogleCalendarService googleCalendarService,
            BookingAvailabilityService availabilityService)
        {
            _context = context;
            _googleCalendarService = googleCalendarService;
            _availabilityService = availabilityService;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            string? status)
        {
            var bookings = _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.GroomingService)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bookings = bookings.Where(b =>
                    (b.Owner != null &&
                     b.Owner.FullName.Contains(searchString)) ||
                    (b.Pet != null &&
                     b.Pet.PetName.Contains(searchString)) ||
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
            var booking = await _context.Bookings
                .Include(b => b.Pet)
                .Include(b => b.GroomingService)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            var allowedStatuses = new[]
            {
                "Pending",
                "Approved",
                "Rejected",
                "Cancelled"
            };

            if (!allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid booking status.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Check availability before approving.
                if (status == "Approved")
                {
                    if (booking.GroomingService == null ||
                        booking.Pet == null)
                    {
                        TempData["Error"] =
                            "Unable to approve booking. Pet or service information is missing.";

                        return RedirectToAction(nameof(Index));
                    }

                    var available =
                        await _availabilityService.IsAvailableAsync(
                            booking.AppointmentDate,
                            booking.GroomingService.DurationMinutes,
                            booking.BookingId);

                    if (!available)
                    {
                        TempData["Error"] =
                            "Cannot approve this booking. The selected time is outside operating hours or the two appointment slots are already occupied.";

                        return RedirectToAction(nameof(Index));
                    }
                }

                // Create a Google Calendar event when approved.
                if (status == "Approved" &&
                    string.IsNullOrWhiteSpace(booking.GoogleCalendarEventId))
                {
                    var eventId = await _googleCalendarService.CreateEventAsync(
                        booking.Pet!.PetName,
                        booking.GroomingService!.ServiceName,
                        booking.AppointmentDate,
                        booking.GroomingService.DurationMinutes);

                    booking.GoogleCalendarEventId = eventId;
                }

                // Delete the calendar event when no longer approved.
                else if (status != "Approved" &&
                         !string.IsNullOrWhiteSpace(booking.GoogleCalendarEventId))
                {
                    await _googleCalendarService.DeleteEventAsync(
                        booking.GoogleCalendarEventId);

                    booking.GoogleCalendarEventId = null;
                }

                booking.Status = status;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Booking status updated successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to update the booking or synchronize Google Calendar. Please try again.";

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
