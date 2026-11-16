
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;
using Pawhaven.Services;

[Authorize(Roles = "Customer")]
public class CustomerBookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BookingAvailabilityService _availabilityService;

    public CustomerBookingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        BookingAvailabilityService availabilityService)
    {
        _context = context;
        _userManager = userManager;
        _availabilityService = availabilityService;
    }

    // GET: CustomerBookings
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var bookings = await _context.Bookings
            .Include(b => b.Pet)
            .Include(b => b.GroomingService)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.AppointmentDate)
            .ToListAsync();

        return View(bookings);
    }

    // GET: CustomerBookings/Create
    public async Task<IActionResult> Create(int serviceId)
    {
        var service = await _context.GroomingServices
            .FirstOrDefaultAsync(s =>
                s.ServiceId == serviceId && s.IsActive);

        if (service == null)
            return NotFound();

        await LoadDropdowns(serviceId);

        var booking = new Booking
        {
            ServiceId = serviceId,
            AppointmentDate = DateTime.Today.AddDays(1).AddHours(9)
        };

        return View(booking);
    }

    // GET: CustomerBookings/AvailableSlots
    [HttpGet]
    public async Task<IActionResult> AvailableSlots(
        int serviceId,
        DateTime date)
    {
        var service = await _context.GroomingServices
            .FirstOrDefaultAsync(s =>
                s.ServiceId == serviceId && s.IsActive);

        if (service == null)
            return NotFound();

        var slots = await _availabilityService.GetAvailableSlotsAsync(
            date,
            service.DurationMinutes);

        var result = slots.Select(slot => new
        {
            value = slot.ToString("HH:mm"),
            label = slot.ToString("hh:mm tt")
        });

        return Json(result);
    }

    // POST: CustomerBookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        var userId = _userManager.GetUserId(User);

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p =>
                p.PetId == booking.PetId &&
                p.UserId == userId);

        var service = await _context.GroomingServices
            .FirstOrDefaultAsync(s =>
                s.ServiceId == booking.ServiceId &&
                s.IsActive);

        if (pet == null)
        {
            ModelState.AddModelError(
                "PetId",
                "Please select one of your registered pets.");
        }

        if (service == null)
        {
            ModelState.AddModelError(
                "ServiceId",
                "Please select a valid grooming service.");
        }

        booking.UserId = userId!;
        booking.Status = "Pending";

        ModelState.Remove("UserId");
        ModelState.Remove("Owner");

        if (ModelState.IsValid && pet != null && service != null)
        {
            var available = await _availabilityService.IsAvailableAsync(
                booking.AppointmentDate,
                service.DurationMinutes);

            if (!available)
            {
                ModelState.AddModelError(
                    "AppointmentDate",
                    "This appointment time is unavailable. Please select another time.");
            }
            else
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Your grooming appointment has been submitted!";

                return RedirectToAction(nameof(Index));
            }
        }

        await LoadDropdowns(booking.ServiceId);

        return View(booking);
    }

    // Load pets and services for the booking form
    private async Task LoadDropdowns(int serviceId)
    {
        var userId = _userManager.GetUserId(User);

        var pets = await _context.Pets
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var services = await _context.GroomingServices
            .Where(s => s.IsActive)
            .ToListAsync();

        ViewBag.Pets = new SelectList(
            pets, "PetId", "PetName");

        ViewBag.Services = new SelectList(
            services, "ServiceId", "ServiceName", serviceId);
    }
}
