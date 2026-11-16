using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawhaven.Data;
using Pawhaven.Models;

[Authorize]
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
    [HttpGet]
public async Task<IActionResult> Create()
{
    var userId = _userManager.GetUserId(User);

    var pets = await _context.Pets
        .Where(p => p.UserId == userId)
        .ToListAsync();

    return View();
}
}