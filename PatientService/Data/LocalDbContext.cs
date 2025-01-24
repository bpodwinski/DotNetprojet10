using Microsoft.EntityFrameworkCore;
using PatientService.Domain;

namespace PatientService.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure PatientDomain Gender enum to be stored as string
            builder.Entity<PatientDomain>()
                .Property(p => p.Gender)
                .HasConversion<string>();

            // Seed initial data
            builder.Entity<PatientDomain>().HasData(
                new PatientDomain
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "TestNone",
                    DateOfBirth = new DateTime(1966, 12, 31),
                    Gender = Gender.Female,
                    Address = "1 Brookside St",
                    PhoneNumber = "100-222-3333"
                },
                new PatientDomain
                {
                    Id = 2,
                    FirstName = "Test",
                    LastName = "TestBorderline",
                    DateOfBirth = new DateTime(1945, 6, 24),
                    Gender = Gender.Male,
                    Address = "2 High St",
                    PhoneNumber = "200-333-4444"
                },
                new PatientDomain
                {
                    Id = 3,
                    FirstName = "Test",
                    LastName = "TestInDanger",
                    DateOfBirth = new DateTime(2004, 6, 18),
                    Gender = Gender.Male,
                    Address = "3 Club Road",
                    PhoneNumber = "300-444-5555"
                },
                new PatientDomain
                {
                    Id = 4,
                    FirstName = "Test",
                    LastName = "TestEarlyOnset",
                    DateOfBirth = new DateTime(2002, 6, 28),
                    Gender = Gender.Female,
                    Address = "4 Valley Dr",
                    PhoneNumber = "400-555-6666"
                }
            );
        }

        public DbSet<PatientDomain> Patients { get; set; }

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
