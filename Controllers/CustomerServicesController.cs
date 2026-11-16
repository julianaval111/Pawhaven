
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;

namespace Pawhaven.Controllers
{
    public class CustomerServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerServicesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var services = _context.GroomingServices
                .AsNoTracking()
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                services = services.Where(s =>
                    s.ServiceName.Contains(searchString) ||
                    (s.Description != null &&
                     s.Description.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            var result = await services
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            return View(result);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.GroomingServices
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == id && s.IsActive);

            if (service == null)
                return NotFound();

            return View(service);
        }
    }
}