using System.ComponentModel.DataAnnotations;

namespace iCareWebApplication.Models
{
    // Represents a patient entity in the iCare web application, storing personal and medical information.
    public class Patient
    {
        // Unique identifier for the Patient entity in the database.
        public int PatientId { get; set; }

        // Full name of the patient.
        // Example: "John Doe"
        public string Name { get; set; }

        // Date of birth of the patient.
        // Example: 1985-07-15
        public DateTime DateOfBirth { get; set; }

        // Address of the patient, which can be null if not provided.
        // Example: "123 Main St, Springfield"
        public string? Address { get; set; }

        // Phone number of the patient, which can be null if not provided.
        // Example: "+1-555-123-4567"
        public string? PhoneNumber { get; set; }

        // Height of the patient in meters or other specified units.
        // Example: 1.75 (for 1.75 meters)
        public float Height { get; set; }

        // Weight of the patient in kilograms or other specified units.
        // Example: 68.5 (for 68.5 kilograms)
        public float Weight { get; set; }

        // Blood group of the patient.
        // Example: "O+", "A-", etc.
        public string BloodGroup { get; set; }

        // Identifier for the bed assigned to the patient.
        // Example: "B12" for Bed 12
        public string BedId { get; set; }

        // Area within the facility where the patient is receiving treatment.
        // Example: "ICU" or "General Ward"
        public string TreatmentArea { get; set; }

        // Geographic code identifier for the patient's location, with validation.
        // Must be between 1 and 50. If the value is outside this range, an error message will be shown.
        [Range(1, 50, ErrorMessage = "GeoCodeId must be between 1 and 50.")]
        public int GeoCodeId { get; set; }

        // Count of documents associated with the patient.
        // Automatically updates based on documents created for the patient.
        public int DocumentCount { get; set; }
    }
}
