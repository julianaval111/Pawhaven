using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalPets = await _context.Pets.CountAsync();

            ViewBag.TotalServices = await _context.GroomingServices.CountAsync();

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.PendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Pending");

            ViewBag.RecentBookings = await _context.Bookings
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.GroomingService)
                .OrderByDescending(b => b.AppointmentDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}