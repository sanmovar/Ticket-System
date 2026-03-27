using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Data;
using Ticket_System.Models;

namespace Ticket_System.Controllers
{
    [Authorize] // nur angemeldete Benutzer dürfen Tickets sehen und erstellen
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // UserManager brauchen wir um den aktuellen Benutzer zu ermitteln
        public TicketsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Tickets
        public async Task<IActionResult> Index(int? projektId, string? zugewiesenerBenutzerId,
                                        string? erstellerId, int pageSize = 10, int page = 1)
        {
            var query = _context.Tickets
                .Include(t => t.Projekt)
                .Include(t => t.Ersteller)
                .Include(t => t.ZugewiesenerBenutzer)
                .AsQueryable();

            if (projektId.HasValue)
                query = query.Where(t => t.ProjektId == projektId.Value);

            if (zugewiesenerBenutzerId == "none")
                query = query.Where(t => t.ZugewiesenerBenutzerId == null);
            else if (!string.IsNullOrEmpty(zugewiesenerBenutzerId))
                query = query.Where(t => t.ZugewiesenerBenutzerId == zugewiesenerBenutzerId);

            if (!string.IsNullOrEmpty(erstellerId))
                query = query.Where(t => t.ErstellerId == erstellerId);

            query = query
                .OrderBy(t => t.Projekt.Titel)
                .ThenByDescending(t => t.ErstelltAm);

            // Pagination
            int totalCount = await query.CountAsync();
            var tickets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Projekte = new SelectList(_context.Projects.ToList(), "Id", "Titel", projektId);
            ViewBag.Benutzer = new SelectList(_userManager.Users.ToList(), "Id", "UserName");

            var model = new TicketFilterDto
            {
                ProjektId = projektId,
                ZugewiesenerBenutzerId = zugewiesenerBenutzerId,
                ErstellerId = erstellerId,
                PageSize = pageSize,
                Page = page,
                TotalCount = totalCount,
                Tickets = tickets
            };

            return View(model);
        }



        // GET: /Tickets/Details/5 → Detailseite eines Tickets
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Projekt)
                .Include(t => t.Ersteller)
                .Include(t => t.ZugewiesenerBenutzer)
                .Include(t => t.GeschlossenVon)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Ersteller) 
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // GET: /Tickets/Create → Formular zum Erstellen
        public IActionResult Create()
        {
            // Nur aktive Projekte: kein Enddatum ODER Enddatum liegt in der Zukunft
            var aktiveProjekte = _context.Projects
                .Where(p => p.Enddatum == null || p.Enddatum > DateTime.Now)
                .ToList();

            // Alle Benutzer für Zuweisung
            var alleBenutzer = _userManager.Users.ToList();

            // ViewBag = Daten die wir zusätzlich an die View übergeben
            ViewBag.ProjektId = new SelectList(aktiveProjekte, "Id", "Titel");
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName");

            return View();
        }

        // POST: /Tickets/Create → Ticket speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            // Ersteller und Erstellungsdatum automatisch setzen
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            ticket.ErstellerId = aktuellerBenutzer.Id;
            ticket.ErstelltAm = DateTime.Now;

            // Zuweisungsdatum automatisch setzen wenn ein Benutzer zugewiesen wurde
            if (!string.IsNullOrEmpty(ticket.ZugewiesenerBenutzerId))
            {
                ticket.ZugewiesenAm = DateTime.Now;
            }

            // ModelState für automatisch gesetzte Felder zurücksetzen
            // (sonst denkt ASP.NET sie fehlen und zeigt Fehler)
            ModelState.Remove("ErstellerId");
            ModelState.Remove("Ersteller");

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Ticket wurde erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));
            }

            // Falls Fehler: Dropdowns neu befüllen
            var aktiveProjekte = _context.Projects
                .Where(p => p.Enddatum == null || p.Enddatum > DateTime.Now)
                .ToList();
            var alleBenutzer = _userManager.Users.ToList();
            ViewBag.ProjektId = new SelectList(aktiveProjekte, "Id", "Titel", ticket.ProjektId);
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName", ticket.ZugewiesenerBenutzerId);

            return View(ticket);
        }
        // GET: /Tickets/Edit/5 → Bearbeitungsformular anzeigen
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Ersteller)
                .Include(t => t.ZugewiesenerBenutzer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Zugriffsprüfung: nur Admin, Ersteller oder Zugewiesener darf bearbeiten
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer.Id;
            bool istZugewiesener = ticket.ZugewiesenerBenutzerId == aktuellerBenutzer.Id;

            if (!istAdmin && !istErsteller && !istZugewiesener)
                return RedirectToAction("AccessDenied", "Account");

            // Nur Benutzer-Dropdown brauchen wir hier
            var alleBenutzer = _userManager.Users.ToList();
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName", ticket.ZugewiesenerBenutzerId);

            return View(ticket);
        }

        // POST: /Tickets/Edit/5 → Änderungen speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Beschreibung, string? ZugewiesenerBenutzerId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            // Zugriffsprüfung
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer.Id;
            bool istZugewiesener = ticket.ZugewiesenerBenutzerId == aktuellerBenutzer.Id;

            if (!istAdmin && !istErsteller && !istZugewiesener)
                return RedirectToAction("AccessDenied", "Account");

            // Nur Beschreibung und Zugewiesener dürfen geändert werden
            ticket.Beschreibung = Beschreibung;

            // Wenn Zugewiesener geändert wurde → Datum aktualisieren
            if (ticket.ZugewiesenerBenutzerId != ZugewiesenerBenutzerId)
            {
                ticket.ZugewiesenerBenutzerId = ZugewiesenerBenutzerId;
                ticket.ZugewiesenAm = string.IsNullOrEmpty(ZugewiesenerBenutzerId)
                    ? null
                    : DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Ticket wurde erfolgreich gespeichert!";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            // Zugriffsprüfung
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer.Id;
            bool istZugewiesener = ticket.ZugewiesenerBenutzerId == aktuellerBenutzer.Id;

            if (!istAdmin && !istErsteller && !istZugewiesener)
                return RedirectToAction("AccessDenied", "Account");

            // Ticket schließen
            ticket.GeschlossenVonId = aktuellerBenutzer.Id;
            ticket.GeschlossenAm = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "🔒 Ticket wurde erfolgreich geschlossen!";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }


        // POST: /Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string inhalt)
        {
            if (string.IsNullOrWhiteSpace(inhalt))
            {
                TempData["Error"] = "Kommentar darf nicht leer sein.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var comment = new Comment
            {
                TicketId = ticketId,
                Inhalt = inhalt.Trim(),
                ErstellerId = user.Id,
                Erstellzeitpunkt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kommentar wurde hinzugefuegt.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }


        // GET: /Tickets/Delete/5 → Bestätigungsseite anzeigen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Projekt)
                .Include(t => t.ZugewiesenerBenutzer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Zugriffsprüfung: nur Admin oder Ersteller darf löschen
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer.Id;

            if (!istAdmin && !istErsteller)
                return RedirectToAction("AccessDenied", "Account");

            return View(ticket);
        }

        // POST: /Tickets/Delete/5 → Ticket wirklich löschen
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            // Zugriffsprüfung
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer.Id;

            if (!istAdmin && !istErsteller)
                return RedirectToAction("AccessDenied", "Account");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ticket wurde erfolgreich gelöscht!";
            return RedirectToAction(nameof(Index));
        }



    }
}
