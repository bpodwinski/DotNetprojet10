using Microsoft.AspNetCore.Identity;
using AuthService.Domain;

namespace AuthService.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static async Task InitializeUser(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDomain>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var adminPassword = configuration["AppSettings:AdminPassword"];
                if (string.IsNullOrEmpty(adminPassword))
                {
                    throw new Exception("Admin password is not configured in appsettings.json");
                }

                // Ensure roles exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin", NormalizedName = "ADMIN" });
                    logger.LogInformation("Role 'Admin' created.");
                }

                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = "User", NormalizedName = "USER" });
                    logger.LogInformation("Role 'User' created.");
                }

                // Ensure admin user exists
                var adminExists = await userManager.GetUsersInRoleAsync("Admin");
                if (adminExists.Count == 0)
                {
                    var adminUser = new UserDomain
                    {
                        UserName = "admin",
                        FullName = "Super Admin",
                        Role = "Admin",
                        NormalizedUserName = "ADMIN",
                        Email = "admin@example.com",
                        NormalizedEmail = "ADMIN@EXAMPLE.COM",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin user created successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing roles and admin user");
            }
        }
    }
}
