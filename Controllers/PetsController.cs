using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

namespace Pawhaven.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
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
                    (p.Owner != null && p.Owner.FullName.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await pets.OrderBy(p => p.PetName).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await LoadOwners();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == pet.UserId))
            {
                ModelState.AddModelError("UserId", "Please select a valid owner.");
            }

            if (ModelState.IsValid)
            {
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Pet added successfully.";

                return RedirectToAction(nameof(Index));
            }

            await LoadOwners(pet.UserId);
            return View(pet);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pet = await _context.Pets.FindAsync(id);

            if (pet == null)
                return NotFound();

            await LoadOwners(pet.UserId);
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet)
        {
            if (id != pet.PetId)
                return NotFound();

            if (!await _context.Users.AnyAsync(u => u.Id == pet.UserId))
            {
                ModelState.AddModelError("UserId", "Please select a valid owner.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Pets.FindAsync(id);

                if (existing == null)
                    return NotFound();

                existing.PetName = pet.PetName;
                existing.PetType = pet.PetType;
                existing.Breed = pet.Breed;
                existing.Age = pet.Age;
                existing.UserId = pet.UserId;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Pet updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            await LoadOwners(pet.UserId);
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _context.Pets.FindAsync(id);

            if (pet == null)
                return NotFound();

            bool hasBookings = await _context.Bookings
                .AnyAsync(b => b.PetId == id);

            if (hasBookings)
            {
                TempData["Error"] =
                    "This pet cannot be deleted because it has booking history.";

                return RedirectToAction(nameof(Index));
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pet deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadOwners(string? selectedId = null)
        {
            var owners = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.UserId = new SelectList(owners, "Id", "FullName", selectedId);
        }
    }
}