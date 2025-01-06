using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PatientService.Domain;

namespace PatientService.Data
{
    public class LocalDbContext : IdentityDbContext<UserDomain, IdentityRole<int>, int>
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure PatientDomain Gender enum to be stored as string
            builder.Entity<PatientDomain>()
                .Property(p => p.Gender)
                .HasConversion<string>();
        }

        public DbSet<PatientDomain> Patients { get; set; }
    }
}