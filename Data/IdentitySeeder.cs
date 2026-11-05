
using Microsoft.AspNetCore.Identity;
using Pawhaven.Models;

namespace Pawhaven.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            const string roleName = "Admin";

            // Create Admin role if it does not exist
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Get admin credentials from Codespaces Secrets
            var username = configuration["AdminAccount:Username"];
            var email = configuration["AdminAccount:Email"];
            var password = configuration["AdminAccount:Password"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                email = $"{username}@pawhaven.local";
            }

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = "Pawhaven Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        result.Errors.Select(e => e.Description)));
                }
            }
            else if (admin.UserName != username)
            {
                var result = await userManager.SetUserNameAsync(admin, username);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        result.Errors.Select(e => e.Description)));
                }
            }

            // Assign Admin role
            if (!await userManager.IsInRoleAsync(admin, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(admin, roleName);

                if (!roleResult.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
