using Microsoft.AspNetCore.Identity;

namespace Ticket_System.Data
{
    // Diese Klasse legt beim Start der App automatisch Rollen und Benutzer an
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // --- Rollen anlegen ---
            string[] roles = { "Admin", "Developer", "Tester" };

            foreach (var role in roles)
            {
                // Nur anlegen, wenn die Rolle noch nicht existiert
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // --- Benutzer anlegen ---
            await CreateUserAsync(userManager, "admin@ticket.de", "Admin123", "Admin");
            await CreateUserAsync(userManager, "dev@ticket.de", "Admin123", "Developer");
            await CreateUserAsync(userManager, "tester@ticket.de", "Admin123", "Tester");
            await CreateUserAsync(userManager, "admin1@ticket.de", "Admin123", "Admin");
            await CreateUserAsync(userManager, "dev1@ticket.de", "Admin123", "Developer");
            await CreateUserAsync(userManager, "tester1@ticket.de", "Admin123", "Tester");
        }

        // Hilfsmethode: Benutzer erstellen und Rolle zuweisen
        private static async Task CreateUserAsync(
            UserManager<IdentityUser> userManager,
            string email, string password, string role)
        {
            // Nur anlegen, wenn der Benutzer noch nicht existiert
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
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
