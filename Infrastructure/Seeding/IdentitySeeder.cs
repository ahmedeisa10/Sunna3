using Microsoft.AspNetCore.Identity;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;

namespace Tamkeen.Infrastructure.Seeding
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ================= ROLES =================
            var roles = Enum.GetNames(typeof(UserRole));

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
