using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Data;
using Ticket_System.Models;

namespace Ticket_System.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Zentrale Zugriffsprüfung — Admin, Ersteller oder Zugewiesener
        private async Task<bool> HatZugriff(Ticket ticket)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "Admin")
                || ticket.ErstellerId == user.Id
                || ticket.ZugewiesenerBenutzerId == user.Id;
        }

        // GET: /Tickets
        public async Task<IActionResult> Index(int? projektId, string? zugewiesenerBenutzerId,
                                string? erstellerId, string? status,
                                int pageSize = 10, int page = 1)
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

            if (!string.IsNullOrEmpty(status) && TicketStatus.Alle.Contains(status))
                query = query.Where(t => t.Status == status);

            query = query
                .OrderBy(t => t.Projekt.Titel)
                .ThenByDescending(t => t.ErstelltAm);

            int totalCount = await query.CountAsync();
            var tickets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Projekte = new SelectList(_context.Projects.ToList(), "Id", "Titel", projektId);
            ViewBag.Benutzer = new SelectList(_userManager.Users.ToList(), "Id", "UserName");

            ViewBag.StatusListe = new SelectList(
                                        TicketStatus.Alle.Select(s => new SelectListItem { Value = s, Text = s }),
                                        "Value", "Text", status);

            var model = new TicketFilterDto
            {
                ProjektId = projektId,
                ZugewiesenerBenutzerId = zugewiesenerBenutzerId,
                ErstellerId = erstellerId,
                Status = status,        
                PageSize = pageSize,
                Page = page,
                TotalCount = totalCount,
                Tickets = tickets
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Projekt)
                .Include(t => t.Ersteller)
                .Include(t => t.ZugewiesenerBenutzer)
                .Include(t => t.GeschlossenVon)
                .Include(t => t.WirdBlockiertDurch)
                    .ThenInclude(a => a.BlockierendesTicket)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Ersteller)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            ticket.Comments = ticket.Comments
                .OrderByDescending(c => c.Erstellzeitpunkt)
                .ToList();

            ViewBag.AlleTickets = await _context.Tickets
                .Where(t => t.Id != id && t.ProjektId == ticket.ProjektId)
                .OrderBy(t => t.Titel)
                .ToListAsync();

            return View(ticket);
        }

        // GET: /Tickets/Create
        public IActionResult Create()
        {
            var aktiveProjekte = _context.Projects
                .Where(p => p.Enddatum == null || p.Enddatum > DateTime.Now)
                .ToList();

            var alleBenutzer = _userManager.Users.ToList();

            ViewBag.ProjektId = new SelectList(aktiveProjekte, "Id", "Titel");
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName");

            return View();
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            ticket.ErstellerId = aktuellerBenutzer!.Id;
            ticket.ErstelltAm = DateTime.Now;

            if (!string.IsNullOrEmpty(ticket.ZugewiesenerBenutzerId))
                ticket.ZugewiesenAm = DateTime.Now;

            ModelState.Remove("ErstellerId");
            ModelState.Remove("Ersteller");

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Ticket wurde erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));
            }

            var aktiveProjekte = _context.Projects
                .Where(p => p.Enddatum == null || p.Enddatum > DateTime.Now)
                .ToList();
            var alleBenutzer = _userManager.Users.ToList();
            ViewBag.ProjektId = new SelectList(aktiveProjekte, "Id", "Titel", ticket.ProjektId);
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName", ticket.ZugewiesenerBenutzerId);

            return View(ticket);
        }

        // GET: /Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Ersteller)
                .Include(t => t.ZugewiesenerBenutzer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            if (!await HatZugriff(ticket))
                return RedirectToAction("AccessDenied", "Account");

            var alleBenutzer = _userManager.Users.ToList();
            ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer, "Id", "UserName", ticket.ZugewiesenerBenutzerId);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Beschreibung, string? ZugewiesenerBenutzerId, string Status)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            if (!await HatZugriff(ticket))
                return RedirectToAction("AccessDenied", "Account");

            ticket.Beschreibung = Beschreibung;

            if (ticket.ZugewiesenerBenutzerId != ZugewiesenerBenutzerId)
            {
                ticket.ZugewiesenerBenutzerId = ZugewiesenerBenutzerId;
                ticket.ZugewiesenAm = string.IsNullOrEmpty(ZugewiesenerBenutzerId)
                    ? null
                    : DateTime.Now;
            }

            // ✅ Status nur setzen wenn gültiger Wert
            if (TicketStatus.Alle.Contains(Status))
            {
                // Blocker-Check nur wenn auf Gelöst gesetzt wird
                if (Status == TicketStatus.Geloest)
                {
                    var offeneBlocker = await _context.TicketAbhaengigkeiten
                        .Include(a => a.BlockierendesTicket)
                        .Where(a => a.BlockiertesTicketId == ticket.Id
                                 && a.BlockierendesTicket.Status != TicketStatus.Geloest)
                        .ToListAsync();

                    if (offeneBlocker.Any())
                    {
                        var titel = string.Join(", ", offeneBlocker.Select(b => $"#{b.BlockierendesTicketId}"));
                        ModelState.AddModelError("Status",
                            $"Ticket kann nicht gelöst werden. Folgende Blocker sind noch offen: {titel}");

                        var alleBenutzer2 = _userManager.Users.ToList();
                        ViewBag.ZugewiesenerBenutzerId = new SelectList(alleBenutzer2, "Id", "UserName", ticket.ZugewiesenerBenutzerId);
                        return View(ticket);
                    }
                }

                ticket.Status = Status;
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

            // ✅ Hilfsmethode statt dupliziertem Block
            if (!await HatZugriff(ticket))
                return RedirectToAction("AccessDenied", "Account");

            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            ticket.Status = TicketStatus.Geloest;
            ticket.GeschlossenVonId = aktuellerBenutzer!.Id;
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
                ErstellerId = user!.Id,
                Erstellzeitpunkt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kommentar wurde hinzugefuegt.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // GET: /Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Projekt)
                .Include(t => t.ZugewiesenerBenutzer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var aktuellerBenutzer = await _userManager.GetUserAsync(User);
            bool istAdmin = await _userManager.IsInRoleAsync(aktuellerBenutzer!, "Admin");
            bool istErsteller = ticket.ErstellerId == aktuellerBenutzer!.Id;

            if (!istAdmin && !istErsteller)
                return RedirectToAction("AccessDenied", "Account");

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets
                        .Include(t => t.Comments)
                        .Include(t => t.WirdBlockiertDurch)
                        .Include(t => t.BlockiertAndere)
                        .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            _context.TicketAbhaengigkeiten.RemoveRange(ticket.WirdBlockiertDurch);
            _context.TicketAbhaengigkeiten.RemoveRange(ticket.BlockiertAndere);
            _context.Comments.RemoveRange(ticket.Comments);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            TempData["Erfolg"] = "Ticket wurde erfolgreich gelöscht.";
            return RedirectToAction(nameof(Index));
        }

        // Blocker hinzufügen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockierungHinzufuegen(int ticketId, int blockerTicketId)
        {
            if (blockerTicketId == 0)
            {
                TempData["Error"] = "Bitte ein blockierendes Ticket auswählen.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            if (ticketId == blockerTicketId)
            {
                TempData["Error"] = "Ein Ticket kann sich nicht selbst blockieren.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var zirkular = await _context.TicketAbhaengigkeiten
                .AnyAsync(a => a.BlockiertesTicketId == blockerTicketId
                            && a.BlockierendesTicketId == ticketId);
            if (zirkular)
            {
                TempData["Error"] = "Zirkuläre Abhängigkeit! Das andere Ticket wird bereits von diesem blockiert.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var existiert = await _context.TicketAbhaengigkeiten
                .AnyAsync(a => a.BlockiertesTicketId == ticketId
                            && a.BlockierendesTicketId == blockerTicketId);
            if (!existiert)
            {
                _context.TicketAbhaengigkeiten.Add(new TicketAbhaengigkeit
                {
                    BlockiertesTicketId = ticketId,
                    BlockierendesTicketId = blockerTicketId
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Blockierung hinzugefügt.";
            }
            else
            {
                TempData["Error"] = "Diese Blockierung existiert bereits.";
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // Blocker entfernen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockierungEntfernen(int ticketId, int blockerTicketId)
        {
            var eintrag = await _context.TicketAbhaengigkeiten
                .FirstOrDefaultAsync(a => a.BlockiertesTicketId == ticketId
                                       && a.BlockierendesTicketId == blockerTicketId);

            if (eintrag != null)
            {
                _context.TicketAbhaengigkeiten.Remove(eintrag);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Blockierung entfernt.";
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
    }
}