using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Metadata;
using iCareWebApplication.Models;


namespace iCareWebApplication.Data
{
    public class iCareContext : DbContext
    {
        public iCareContext(DbContextOptions<iCareContext> options) : base(options) { }

        // Define DbSet for each entity/table in the database
        public DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<PatientAssignment> PatientAssignment { get; set; }
        public DbSet<PatientTreatment> PatientTreatments { get; set; }
        public DbSet<iCareDocument> iCareDocuments { get; set; }
        public DbSet<ModificationHistory> ModificationHistories { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        //public object Drug { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Optional: Configure entity relationships here
        }
    }
}
