
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

namespace Pawhaven.Controllers
{
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PetsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Pets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pets.ToListAsync());
        }

        // GET: Pets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
                return NotFound();

            return View(pet);
        }

        // GET: Pets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Species,Breed,Age,Size")] Pet pet,
            IFormFile? pictureFile)
        {
            if (pictureFile != null && pictureFile.Length > 0)
            {
                if (!IsValidImage(pictureFile))
                {
                    ModelState.AddModelError(
                        "PicturePath",
                        "Please upload a JPG, PNG, GIF, or WEBP image under 5 MB.");
                }
            }

            if (!ModelState.IsValid)
                return View(pet);

            if (pictureFile != null && pictureFile.Length > 0)
            {
                pet.PicturePath = await SavePictureAsync(pictureFile);
            }

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pet = await _context.Pets.FindAsync(id);

            if (pet == null)
                return NotFound();

            return View(pet);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("PetId,Name,Species,Breed,Age,Size")] Pet pet,
            IFormFile? pictureFile)
        {
            if (id != pet.PetId)
                return NotFound();

            var existingPet = await _context.Pets.FindAsync(id);

            if (existingPet == null)
                return NotFound();

            if (pictureFile != null && pictureFile.Length > 0)
            {
                if (!IsValidImage(pictureFile))
                {
                    ModelState.AddModelError(
                        "PicturePath",
                        "Please upload a JPG, PNG, GIF, or WEBP image under 5 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                pet.PicturePath = existingPet.PicturePath;
                return View(pet);
            }

            existingPet.Name = pet.Name;
            existingPet.Species = pet.Species;
            existingPet.Breed = pet.Breed;
            existingPet.Age = pet.Age;
            existingPet.Size = pet.Size;

            if (pictureFile != null && pictureFile.Length > 0)
            {
                DeletePicture(existingPet.PicturePath);
                existingPet.PicturePath = await SavePictureAsync(pictureFile);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Pets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
                return NotFound();

            return View(pet);
        }

        // POST: Pets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Pets.FindAsync(id);

            if (pet != null)
            {
                DeletePicture(pet.PicturePath);

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            return file.Length <= 5 * 1024 * 1024
                && allowedExtensions.Contains(extension);
        }

        private async Task<string> SavePictureAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "uploads",
                "pets");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/pets/{fileName}";
        }

        private void DeletePicture(string? picturePath)
        {
            if (string.IsNullOrWhiteSpace(picturePath))
                return;

            var fileName = Path.GetFileName(picturePath);
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "uploads",
                "pets");

            var filePath = Path.Combine(uploadsFolder, fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}
