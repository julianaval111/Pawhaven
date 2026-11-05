using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroomingServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroomingServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ AND SEARCH
        public async Task<IActionResult> Index(string? searchString)
        {
            var services = _context.GroomingServices
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                services = services.Where(s =>
                    s.ServiceName.Contains(searchString) ||
                    (s.Description != null &&
                     s.Description.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await services
                .OrderBy(s => s.ServiceName)
                .ToListAsync());
        }

        // CREATE - DISPLAY FORM
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - SAVE DATA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroomingService service)
        {
            bool duplicate = await _context.GroomingServices
                .AnyAsync(s => s.ServiceName == service.ServiceName);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "ServiceName",
                    "A service with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.GroomingServices.Add(service);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Service added successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }

        // EDIT - DISPLAY FORM
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.GroomingServices
                .FindAsync(id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        // EDIT - SAVE CHANGES
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroomingService service)
        {
            if (id != service.ServiceId)
                return NotFound();

            bool duplicate = await _context.GroomingServices
                .AnyAsync(s =>
                    s.ServiceName == service.ServiceName &&
                    s.ServiceId != id);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "ServiceName",
                    "Another service already uses this name.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.GroomingServices.FindAsync(id);

                if (existing == null)
                    return NotFound();

                existing.ServiceName = service.ServiceName;
                existing.Description = service.Description;
                existing.Price = service.Price;
                existing.IsActive = service.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Service updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }

        // DELETE / DEACTIVATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.GroomingServices.FindAsync(id);

            if (service == null)
                return NotFound();

            service.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Service deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}