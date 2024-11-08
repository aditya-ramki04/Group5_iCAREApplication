namespace iCareWebApplication.Models
{
    // Represents an assignment of a patient to a healthcare worker in the iCare web application.
    // Tracks which worker is responsible for which patient, along with assignment details.
    public class PatientAssignment
    {
        // Unique identifier for the PatientAssignment entity in the database.
        public int PatientAssignmentId { get; set; }

        // Identifier for the patient being assigned.
        // Links this assignment record to a specific patient.
        public int PatientId { get; set; }

        // Identifier for the worker assigned to the patient.
        // Links this assignment record to a specific healthcare worker.
        public int WorkerId { get; set; }

        // Date and time when the patient was assigned to the worker.
        public DateTime DateAssigned { get; set; }

        // Indicates whether the assignment is currently active.
        // True if the assignment is active, false if it has ended.
        public bool Active { get; set; }
    }
}
