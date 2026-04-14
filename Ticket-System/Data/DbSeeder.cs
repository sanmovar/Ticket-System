using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Models;

namespace Ticket_System.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<AppDbContext>();

            // Tickets ohne gültigen Status auf "Offen" setzen
            var gueltigeStatus = TicketStatus.Alle;
            var ungueltigeTickets = await db.Tickets
                .Where(t => !gueltigeStatus.Contains(t.Status))
                .ToListAsync();

            if (ungueltigeTickets.Any())
            {
                foreach (var t in ungueltigeTickets)
                    t.Status = TicketStatus.Offen;
                await db.SaveChangesAsync();
            }

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // --- Rollen anlegen ---
            string[] roles = { "Admin", "Developer", "Tester" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- Benutzer anlegen ---
            await CreateUserAsync(userManager, "admin@ticket.de", "Admin123", "Admin");
            await CreateUserAsync(userManager, "dev@ticket.de", "Admin123", "Developer");
            await CreateUserAsync(userManager, "tester@ticket.de", "Admin123", "Tester");
            await CreateUserAsync(userManager, "admin1@ticket.de", "Admin123", "Admin");
            await CreateUserAsync(userManager, "dev1@ticket.de", "Admin123", "Developer");
            await CreateUserAsync(userManager, "tester1@ticket.de", "Admin123", "Tester");

            // --- Projekte und Tickets anlegen ---
            if (await db.Projects.AnyAsync()) return;

            var admin = await userManager.FindByEmailAsync("admin@ticket.de");
            var dev = await userManager.FindByEmailAsync("dev@ticket.de");
            var tester = await userManager.FindByEmailAsync("tester@ticket.de");
            var dev1 = await userManager.FindByEmailAsync("dev1@ticket.de");

            var projektDaten = new[]
            {
                ("Online-Shop Relaunch",       "Neugestaltung des gesamten Online-Shops mit modernem UI."),
                ("Mitarbeiterportal",           "Internes Portal für HR, Urlaub und Gehaltsabrechnungen."),
                ("Mobile App v2.0",             "Überarbeitung der mobilen Anwendung für iOS und Android."),
                ("Datenbank-Migration",         "Migration der Legacy-Datenbank auf PostgreSQL."),
                ("API-Gateway",                 "Entwicklung eines zentralen API-Gateways für Microservices."),
                ("Reporting Dashboard",         "Echtzeit-Dashboard für Verkaufs- und Nutzungsstatistiken."),
                ("Authentifizierungsmodul",     "Implementierung von OAuth2 und Zwei-Faktor-Authentifizierung."),
                ("Lagerverwaltungssystem",      "Digitalisierung der Lagerprozesse und Bestandskontrolle."),
                ("Kundenbenachrichtigungen",    "Automatisches E-Mail- und SMS-Benachrichtigungssystem."),
                ("Performance-Optimierung",     "Analyse und Optimierung der Serverantwortzeiten.")
            };

            var ticketVorlagen = new[]
            {
                ("Anforderungsanalyse",          TicketStatus.Geloest,       dev),
                ("Backend-Implementierung",      TicketStatus.InBearbeitung, dev1),
                ("Testing und Qualitätssicherung", TicketStatus.Offen,       tester)
            };

            var alleProjekte = new List<Project>();

            for (int i = 0; i < projektDaten.Length; i++)
            {
                var (titel, beschreibung) = projektDaten[i];
                var projekt = new Project
                {
                    Titel = titel,
                    Beschreibung = beschreibung,
                    Startdatum = DateTime.Now.AddMonths(-i - 1),
                    Enddatum = DateTime.Now.AddMonths(6 - i)
                };
                db.Projects.Add(projekt);
                alleProjekte.Add(projekt);
            }

            await db.SaveChangesAsync();

            // Tickets erstellen
            var alleTickets = new List<(Ticket ticket, int projektIndex, int ticketIndex)>();

            for (int pi = 0; pi < alleProjekte.Count; pi++)
            {
                for (int ti = 0; ti < ticketVorlagen.Length; ti++)
                {
                    var (ticketTitel, status, zugewiesener) = ticketVorlagen[ti];
                    var ticket = new Ticket
                    {
                        Titel = $"{ticketTitel} – {alleProjekte[pi].Titel}",
                        Beschreibung = $"Aufgabe '{ticketTitel}' im Rahmen des Projekts '{alleProjekte[pi].Titel}'.",
                        Status = status,
                        ProjektId = alleProjekte[pi].Id,
                        ErstellerId = admin!.Id,
                        ErstelltAm = DateTime.Now.AddDays(-(pi * 3 + ti)),
                        ZugewiesenerBenutzerId = zugewiesener?.Id,
                        ZugewiesenAm = zugewiesener != null ? DateTime.Now.AddDays(-(pi * 3 + ti - 1)) : null
                    };
                    db.Tickets.Add(ticket);
                    alleTickets.Add((ticket, pi, ti));
                }
            }

            await db.SaveChangesAsync();

            // --- Abhängigkeiten: Ticket 3 (Testing) wird durch Ticket 2 (Backend) blockiert ---
            // Das gilt für jeden zweiten Projekt (5 von 10 Projekten)
            for (int pi = 0; pi < alleProjekte.Count; pi += 2)
            {
                var backend = alleTickets.First(t => t.projektIndex == pi && t.ticketIndex == 1).ticket;
                var testing = alleTickets.First(t => t.projektIndex == pi && t.ticketIndex == 2).ticket;

                db.TicketAbhaengigkeiten.Add(new TicketAbhaengigkeit
                {
                    BlockiertesTicketId = testing.Id,   // Testing wird blockiert
                    BlockierendesTicketId = backend.Id    // durch Backend
                });
            }

            await db.SaveChangesAsync();
        }

        private static async Task CreateUserAsync(
            UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}