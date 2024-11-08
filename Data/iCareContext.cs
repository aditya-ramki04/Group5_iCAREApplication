using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Metadata;
using iCareWebApplication.Models;

namespace iCareWebApplication.Data
{
    // The main database context class for the iCare web application.
    // It manages database connections and defines entities in the database.
    public class iCareContext : DbContext
    {
        // Constructor for the iCareContext class
        // Takes in DbContextOptions<iCareContext> and passes them to the base DbContext constructor.
        // Parameters:
        //   options - configuration options for the DbContext, such as connection string.
        public iCareContext(DbContextOptions<iCareContext> options) : base(options) { }

        // DbSets represent tables in the database and allow for querying and saving instances of entities.
        public DbSet<User> User { get; set; }              // Represents the User table in the database
        public DbSet<Role> Roles { get; set; }             // Represents the Role table in the database
        public DbSet<Patient> Patient { get; set; }        // Represents the Patient table in the database
        public DbSet<PatientAssignment> PatientAssignment { get; set; }  // Represents the PatientAssignment table
        public DbSet<PatientTreatment> PatientTreatment { get; set; }    // Represents the PatientTreatment table
        public DbSet<iCareDocument> iCareDocuments { get; set; }         // Represents the iCareDocuments table
        public DbSet<ModificationHistory> ModificationHistories { get; set; }  // Represents the ModificationHistories table
        public DbSet<Drug> Drugs { get; set; }             // Represents the Drug table in the database

        // Configures relationships between entities in the model.
        // This method is called automatically when the model is created and maps relationships in the database schema.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Calls the base implementation of OnModelCreating to initialize any default behavior.
            base.OnModelCreating(modelBuilder);

            // Configure relationship between PatientAssignment and User
            // Sets up a one-to-many relationship between User (worker) and PatientAssignment.
            // A User can have multiple PatientAssignments, but each PatientAssignment links to a single User (WorkerId).
            modelBuilder.Entity<PatientAssignment>()
                .HasOne<User>() // Defines User as the related entity for WorkerId in PatientAssignment
                .WithMany() // Indicates that one User can have multiple PatientAssignments
                .HasForeignKey(pa => pa.WorkerId); // Sets WorkerId as the foreign key in PatientAssignment

            // Configure relationship between PatientAssignment and Patient
            // Sets up a one-to-many relationship where a Patient can have multiple PatientAssignments.
            // Each PatientAssignment links to a single Patient (PatientId).
            modelBuilder.Entity<PatientAssignment>()
                .HasOne<Patient>() // Defines Patient as the related entity for PatientId in PatientAssignment
                .WithMany() // Indicates that one Patient can have multiple PatientAssignments
                .HasForeignKey(pa => pa.PatientId); // Sets PatientId as the foreign key in PatientAssignment
        }
    }
}
