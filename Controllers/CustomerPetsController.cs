
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

[Authorize(Roles = "Customer")]
public class CustomerPetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerPetsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: CustomerPets
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var pets = await _context.Pets
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return View(pets);
    }

    // GET: CustomerPets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.UserId == userId);

        if (pet == null)
        {
            return NotFound();
        }

        return View(pet);
    }

    // GET: CustomerPets/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CustomerPets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pet pet)
    {
        // Assign the logged-in customer as the owner
        pet.UserId = _userManager.GetUserId(User)!;

        // Remove the validation error because UserId is assigned here
        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(pet);
    }

    // GET: CustomerPets/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.UserId == userId);

        if (pet == null)
        {
            return NotFound();
        }

        return View(pet);
    }

    // POST: CustomerPets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pet pet)
    {
        if (id != pet.PetId)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var existingPet = await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.UserId == userId);

        if (existingPet == null)
        {
            return NotFound();
        }

        pet.UserId = userId!;

        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            existingPet.PetName = pet.PetName;
            existingPet.PetType = pet.PetType;
            existingPet.Breed = pet.Breed;
            existingPet.Age = pet.Age;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(pet);
    }

    // GET: CustomerPets/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.UserId == userId);

        if (pet == null)
        {
            return NotFound();
        }

        return View(pet);
    }

    // POST: CustomerPets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.UserId == userId);

        if (pet == null)
        {
            return NotFound();
        }

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.PetId == id);

        if (hasBookings)
        {
            TempData["Error"] = "This pet cannot be deleted because it has existing bookings.";
            return RedirectToAction(nameof(Index));
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}