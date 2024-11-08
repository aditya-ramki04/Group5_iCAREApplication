namespace iCareWebApplication.Models
{
    // Represents a record of treatment provided to a patient in the iCare web application.
    // Tracks details about the treatment, the administering worker, and any associated medication.
    public class PatientTreatment
    {
        // Unique identifier for the PatientTreatment entity in the database.
        public int PatientTreatmentId { get; set; }

        // Identifier for the patient receiving the treatment.
        // Links this treatment record to a specific patient.
        public int PatientId { get; set; }

        // Identifier for the worker administering the treatment.
        // Links this treatment record to a specific healthcare worker.
        public int WorkerId { get; set; }

        // Description of the treatment provided.
        // Example: "Physical therapy session for back pain management."
        public string Description { get; set; }

        // Date and time when the treatment was provided.
        public DateTime TreatmentDate { get; set; }

        // Identifier for the drug administered during the treatment, if applicable.
        // Links this treatment record to a specific medication.
        public int DrugId { get; set; }
    }
}
