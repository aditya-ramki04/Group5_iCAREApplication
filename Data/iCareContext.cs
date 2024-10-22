using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Metadata;

namespace iCareWebApplication.Data
{
    public class iCareContext : DbContext
    {
        public iCareContext(DbContextOptions<iCareContext> options) : base(options) { }

        // Define DbSet for each entity/table in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientAssignment> PatientAssignments { get; set; }
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ModificationHistory> ModificationHistories { get; set; }
        public DbSet<Drug> Drugs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Optional: Configure entity relationships here
        }
    }
}
