
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

[Authorize(Roles = "Staff")]
public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Staff
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();
        ViewBag.PendingBookings = await _context.Bookings
            .CountAsync(b => b.Status == "Pending");

        ViewBag.TotalPets = await _context.Pets.CountAsync();

        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        ViewBag.TotalCustomers = customers.Count;

        return View();
    }

    // GET: Staff/Bookings
    public async Task<IActionResult> Bookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Owner)
            .Include(b => b.Pet)
            .Include(b => b.GroomingService)
            .OrderByDescending(b => b.AppointmentDate)
            .ToListAsync();

        return View(bookings);
    }

    // POST: Staff/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int bookingId, string status)
    {
        string[] allowedStatuses =
        {
            "Pending",
            "Approved",
            "Rejected",
            "Cancelled"
        };

        if (!allowedStatuses.Contains(status))
        {
            return BadRequest();
        }

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = status;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking status updated successfully.";

        return RedirectToAction(nameof(Bookings));
    }

    
    // GET: Staff/Pets
    public async Task<IActionResult> Pets(string searchString)
    {
        var pets = _context.Pets
            .Include(p => p.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            pets = pets.Where(p =>
                p.PetName.Contains(searchString) ||
                p.PetType.Contains(searchString) ||
                (p.Breed != null && p.Breed.Contains(searchString)) ||
                (p.Owner != null &&
                    (p.Owner.FullName.Contains(searchString) ||
                    (p.Owner.Email != null && p.Owner.Email.Contains(searchString))))
            );
        }

        ViewBag.SearchString = searchString;

        var result = await pets
            .OrderBy(p => p.PetName)
            .ToListAsync();

        return View(result);
    }

}
