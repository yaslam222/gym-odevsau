using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SporSalonuProjesi.Data.Entities;

namespace SporSalonuProjesi.Seed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("SeedData");
            logger?.LogInformation("=== SEED PROCESS STARTED ===");

            try
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                logger?.LogInformation("UserManager and RoleManager obtained successfully");

                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, logger);

                logger?.LogInformation("=== SEED PROCESS COMPLETED ===");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "ERROR in SeedData.Initialize");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger?.LogInformation("Starting role seeding...");
            const string adminRole = "Admin";

            var roleExists = await roleManager.RoleExistsAsync(adminRole);
            logger?.LogInformation($"Admin role exists: {roleExists}");

            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(adminRole));
                if (result.Succeeded)
                {
                    logger?.LogInformation("✓ Admin role created successfully");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    logger?.LogError($"✗ Admin role creation FAILED: {errors}");
                }
            }
            else
            {
                logger?.LogInformation("Admin role already exists, skipping creation");
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger?.LogInformation("Starting admin user seeding...");

            var adminEmail = "B191210574@sakarya.edu.tr";
            var adminPass = "sau";

            logger?.LogInformation($"Looking for existing user with email: {adminEmail}");
            var existing = await userManager.FindByEmailAsync(adminEmail);

            if (existing == null)
            {
                logger?.LogInformation("User not found, creating new admin user...");

                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPass);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    logger?.LogError($"✗ Admin user creation FAILED: {errors}");
                    return;
                }

                logger?.LogInformation("✓ Admin user created successfully");

                var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
                if (roleResult.Succeeded)
                {
                    logger?.LogInformation("✓ Admin user added to Admin role successfully");
                }
                else
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    logger?.LogError($"✗ Failed to add user to role: {errors}");
                }
            }
            else
            {
                logger?.LogInformation($"Admin user already exists with ID: {existing.Id}");

                // Check if user is in Admin role
                var isInRole = await userManager.IsInRoleAsync(existing, "Admin");
                logger?.LogInformation($"User is in Admin role: {isInRole}");

                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                    logger?.LogInformation("Added existing user to Admin role");
                }
            }
        }
    }
}