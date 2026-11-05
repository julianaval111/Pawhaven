using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pawhaven.Models;

namespace Pawhaven.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your username and password.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                username,
                password,
                false,
                false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}