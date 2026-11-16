
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

            // Create Admin, Staff, and Customer roles
            string[] roleNames = { "Admin", "Staff", "Customer" };

            foreach (var roleName in roleNames)
            {
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
            }

            // ADMIN ACCOUNT
            var adminUsername = configuration["AdminAccount:Username"];
            var adminEmail = configuration["AdminAccount:Email"];
            var adminPassword = configuration["AdminAccount:Password"];

            if (!string.IsNullOrWhiteSpace(adminUsername) &&
                !string.IsNullOrWhiteSpace(adminPassword))
            {
                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    adminEmail = $"{adminUsername}@pawhaven.local";
                }

                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminUsername,
                        Email = adminEmail,
                        FullName = "Pawhaven Administrator",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                    }
                }
                else if (admin.UserName != adminUsername)
                {
                    var result = await userManager.SetUserNameAsync(
                        admin, adminUsername);

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                    }
                }

                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(
                        admin, "Admin");

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            // STAFF ACCOUNT
            var staffUsername = configuration["StaffAccount:Username"];
            var staffEmail = configuration["StaffAccount:Email"];
            var staffPassword = configuration["StaffAccount:Password"];

            if (!string.IsNullOrWhiteSpace(staffUsername) &&
                !string.IsNullOrWhiteSpace(staffPassword))
            {
                if (string.IsNullOrWhiteSpace(staffEmail))
                {
                    staffEmail = $"{staffUsername}@pawhaven.local";
                }

                var staff = await userManager.FindByEmailAsync(staffEmail);

                if (staff == null)
                {
                    staff = new ApplicationUser
                    {
                        UserName = staffUsername,
                        Email = staffEmail,
                        FullName = "Pawhaven Staff",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(
                        staff, staffPassword);

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                    }
                }
                else if (staff.UserName != staffUsername)
                {
                    var result = await userManager.SetUserNameAsync(
                        staff, staffUsername);

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                    }
                }

                if (!await userManager.IsInRoleAsync(staff, "Staff"))
                {
                    var roleResult = await userManager.AddToRoleAsync(
                        staff, "Staff");

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
