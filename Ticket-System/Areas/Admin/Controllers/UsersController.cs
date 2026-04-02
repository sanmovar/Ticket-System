using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ticket_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            // Für jeden Benutzer die Rollen laden
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Rollen = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        // GET: /Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var alleRollen = _roleManager.Roles.Select(r => r.Name!).ToList();
            var benutzerRollen = await _userManager.GetRolesAsync(user);

            ViewBag.AlleRollen = alleRollen;
            ViewBag.BenutzerRollen = benutzerRollen;

            return View(user);
        }

        // POST: /Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string? neuEmail, List<string> rollen)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // E-Mail / Username aktualisieren
            if (!string.IsNullOrWhiteSpace(neuEmail) && user.Email != neuEmail)
            {
                user.Email = neuEmail;
                user.UserName = neuEmail;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    var alleRollen2 = _roleManager.Roles.Select(r => r.Name!).ToList();
                    var benutzerRollen2 = await _userManager.GetRolesAsync(user);
                    ViewBag.AlleRollen = alleRollen2;
                    ViewBag.BenutzerRollen = benutzerRollen2;
                    return View(user);
                }
            }

            // Rollen aktualisieren: alte entfernen, neue hinzufügen
            var aktuelleRollen = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, aktuelleRollen);
            if (rollen.Any())
                await _userManager.AddToRolesAsync(user, rollen);

            TempData["Success"] = "✅ Benutzer wurde erfolgreich aktualisiert!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Create
        public IActionResult Create()
        {
            ViewBag.AlleRollen = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View();
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string passwort, List<string> rollen)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwort))
            {
                ModelState.AddModelError("", "E-Mail und Passwort sind Pflichtfelder.");
                ViewBag.AlleRollen = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, passwort);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                ViewBag.AlleRollen = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View();
            }

            if (rollen.Any())
                await _userManager.AddToRolesAsync(user, rollen);

            TempData["Success"] = "✅ Benutzer wurde erfolgreich angelegt!";
            return RedirectToAction(nameof(Index));
        }
    }
}