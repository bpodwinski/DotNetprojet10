using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AuthService.Domain;

namespace AuthService.Data
{
    public class LocalDbContext : IdentityDbContext<UserDomain, IdentityRole<int>, int>
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define roles
            builder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN"
            }, new IdentityRole<int> {
                Id = 2,
                Name = "User",
                NormalizedName = "USER"
            });
        }

        public async Task WaitForDatabaseAsync(DbContext dbContext, int maxRetries = 10, int delayInSeconds = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await dbContext.Database.CanConnectAsync();
                    return;
                }
                catch
                {
                    Console.WriteLine($"Database not ready, retrying in {delayInSeconds} seconds... ({i + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
                }
            }

            throw new Exception("Database is not available after multiple retries.");
        }
    }
}
