using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Data;
using Ticket_System.Models;

namespace Ticket_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var statistik = new StatistikDto();

            if (User.Identity!.IsAuthenticated)
            {
                // Ticket-Statistiken
                statistik.TicketsGesamt = await _context.Tickets.CountAsync();
                statistik.TicketsGeschlossen = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.Geloest || t.Status == 
                    TicketStatus.Abgebrochen);
                statistik.TicketsOffen = await _context.Tickets
                    .CountAsync(t => t.Status == TicketStatus.Offen || t.Status == 
                    TicketStatus.InBearbeitung);

                // Projekt-Statistiken
                statistik.ProjekteGesamt = await _context.Projects.CountAsync();
                statistik.ProjekteBeendet = await _context.Projects
                    .CountAsync(p => p.Enddatum != null && p.Enddatum < DateTime.Now);
                statistik.ProjekteAktiv = statistik.ProjekteGesamt - statistik.ProjekteBeendet;

                // Benutzer-Statistiken 
                statistik.BenutzerGesamt = _userManager.Users.Count();
                statistik.AnzahlAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                statistik.AnzahlDeveloper = (await _userManager.GetUsersInRoleAsync("Developer")).Count;
                statistik.AnzahlTester = (await _userManager.GetUsersInRoleAsync("Tester")).Count;
            }

            return View(statistik);
        }
    }
}
