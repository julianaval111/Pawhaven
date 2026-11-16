
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

[Authorize(Roles = "Customer")]
public class CustomerBookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerBookingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.IsActive);

        if (service == null)
        {
            return NotFound();
        }

        await LoadDropdowns(serviceId);

        var booking = new Booking
        {
            ServiceId = serviceId,
            AppointmentDate = DateTime.Today.AddDays(1)
        };

        return View(booking);
    }

    // POST: CustomerBookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        var userId = _userManager.GetUserId(User);

        // Check if the selected pet belongs to the customer
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p =>
                p.PetId == booking.PetId &&
                p.UserId == userId);

        // Check if the selected service is active
        var service = await _context.GroomingServices
            .FirstOrDefaultAsync(s =>
                s.ServiceId == booking.ServiceId &&
                s.IsActive);

        if (pet == null)
        {
            ModelState.AddModelError("PetId", "Please select one of your registered pets.");
        }

        if (service == null)
        {
            ModelState.AddModelError("ServiceId", "Please select a valid grooming service.");
        }

        booking.UserId = userId!;
        booking.Status = "Pending";

        ModelState.Remove("UserId");

        if (ModelState.IsValid && pet != null && service != null)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your grooming appointment has been submitted!";

            return RedirectToAction(nameof(Index));
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

        ViewBag.Pets = new SelectList(pets, "PetId", "PetName");
        ViewBag.Services = new SelectList(services, "ServiceId", "ServiceName", serviceId);
    }
}
