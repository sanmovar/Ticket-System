using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Data;
using Ticket_System.Models;

namespace Ticket_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Projects
        public async Task<IActionResult> Index(int pageSize = 10, int page = 1)
        {
            var query = _context.Projects.AsQueryable();

            int totalCount = await query.CountAsync();
            var projects = await query
                .Include(p => p.Tickets)
                .OrderBy(p => p.Titel)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(projects);
        }

        // GET: Admin/Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.ZugewiesenerBenutzer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // GET: Admin/Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titel,Beschreibung,Startdatum,Enddatum")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Projekt wurde erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Admin/Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Admin/Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Beschreibung,Startdatum,Enddatum")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Projects.Any(e => e.Id == project.Id))
                        return NotFound();
                    throw;
                }

                TempData["Success"] = "Projekt wurde erfolgreich gespeichert!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Admin/Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Admin/Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.Comments)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.WirdBlockiertDurch)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.BlockiertAndere)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            foreach (var ticket in project.Tickets)
            {
                _context.TicketAbhaengigkeiten.RemoveRange(ticket.WirdBlockiertDurch);
                _context.TicketAbhaengigkeiten.RemoveRange(ticket.BlockiertAndere);
                _context.Comments.RemoveRange(ticket.Comments);
            }

            _context.Tickets.RemoveRange(project.Tickets);
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Projekt wurde erfolgreich gelöscht!";
            return RedirectToAction(nameof(Index));
        }
    }
}