using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Data;

namespace Ticket_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UsersController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userRoleMap = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                select new { ur.UserId, RoleName = r.Name! }
            ).ToListAsync();

            var userRoles = userRoleMap
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => (IList<string>)g.Select(x => x.RoleName).ToList()
                );

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

            var alleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
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

            if (!rollen.Any())
            {
                ModelState.AddModelError("", "Der Benutzer muss mindestens eine Rolle haben.");
                ViewBag.AlleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                ViewBag.BenutzerRollen = await _userManager.GetRolesAsync(user);
                return View(user);
            }

            if (!string.IsNullOrWhiteSpace(neuEmail) && user.Email != neuEmail)
            {
                user.Email = neuEmail;
                user.UserName = neuEmail;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    ViewBag.AlleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                    ViewBag.BenutzerRollen = await _userManager.GetRolesAsync(user);
                    return View(user);
                }
            }

            var aktuelleRollen = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, aktuelleRollen);
            await _userManager.AddToRolesAsync(user, rollen);

            TempData["Success"] = "✅ Benutzer wurde erfolgreich aktualisiert!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AlleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View();
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string passwort, List<string> rollen)
        {
            // email prüfen
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "E-Mail ist ein Pflichtfeld.");
            }
            else
            {
                
                var emailRegex = new System.Text.RegularExpressions.Regex(
                    @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (!emailRegex.IsMatch(email))
                {
                    ModelState.AddModelError("", "Die E-Mail-Adresse ist ungültig. Beispiel: benutzer@example.de");
                }
            }

            // Passwort prüfen
            if (string.IsNullOrWhiteSpace(passwort))
            {
                ModelState.AddModelError("", "Passwort ist ein Pflichtfeld.");
            }
            else if (passwort.Length < 6)
            {
                ModelState.AddModelError("", "Das Passwort muss mindestens 6 Zeichen lang sein.");
            }
            else if (!passwort.Any(char.IsUpper))
            {
                ModelState.AddModelError("", "Das Passwort muss mindestens einen Großbuchstaben enthalten.");
            }
            else if (!passwort.Any(char.IsLower))
            {
                ModelState.AddModelError("", "Das Passwort muss mindestens einen Kleinbuchstaben enthalten.");
            }
            else if (!passwort.Any(char.IsDigit))
            {
                ModelState.AddModelError("", "Das Passwort muss mindestens eine Zahl enthalten.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AlleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
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

                ViewBag.AlleRollen = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View();
            }

            if (rollen.Any())
                await _userManager.AddToRolesAsync(user, rollen);

            TempData["Success"] = "✅ Benutzer wurde erfolgreich angelegt!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            if (aktuellerBenutzer != null && user.Id == aktuellerBenutzer.Id)
            {
                TempData["Error"] = "Sie können Ihren eigenen Account nicht löschen.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Rollen = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            if (aktuellerBenutzer != null && user.Id == aktuellerBenutzer.Id)
            {
                TempData["Error"] = "Sie können Ihren eigenen Account nicht löschen.";
                return RedirectToAction(nameof(Index));
            }

            var istAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (istAdmin)
            {
                var alleAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                if (alleAdmins.Count <= 1)
                {
                    TempData["Error"] = "Der letzte Administrator kann nicht gelöscht werden.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "✅ Benutzer wurde erfolgreich gelöscht.";
            return RedirectToAction(nameof(Index));
        }
    }
}