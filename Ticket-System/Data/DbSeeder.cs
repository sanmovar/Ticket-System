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