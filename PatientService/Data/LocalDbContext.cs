using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Data
{
    public class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
    {
        public DbSet<Patient> Patients { get; set; }
    }
}
