using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ticket_System.Models;

namespace Ticket_System.Controllers
{
    public class AccountController : Controller
    {
        // SignInManager: zuständig für Login und Logout
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // GET: /Account/Login  → zeigt das Login-Formular
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login → verarbeitet das ausgefüllte Formular
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            // Fehlermeldung anzeigen wenn Login fehlgeschlagen
            ModelState.AddModelError("", "Ungültige E-Mail oder Passwort.");
            return View(model);
        }

        // POST: /Account/Logout → meldet den Benutzer ab
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied → wird automatisch aufgerufen wenn kein Zugriff
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
