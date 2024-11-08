namespace iCareWebApplication.Models
{
    // Represents a document associated with a patient in the iCare web application.
    // Stores file metadata and tracking information for document management.
    public class iCareDocument
    {
        // Unique identifier for the iCareDocument entity in the database.
        public int iCareDocumentId { get; set; }

        // Identifier for the associated patient.
        // Links this document to a specific patient record.
        public int PatientId { get; set; }

        // File path where the document is stored on the server.
        // Example: "/documents/patient_records/doc123.pdf"
        public string FilePath { get; set; }

        // Type of file, specifying the format of the document.
        // Example: "PDF" or "DOCX"
        public string FileType { get; set; }

        // Identifier for the user who created the document.
        // Tracks which user generated or uploaded the document initially.
        public int CreatedBy { get; set; }

        // Date and time when the document was created.
        public DateTime CreationDate { get; set; }

        // Date and time when the document was last modified.
        // Helps in tracking updates or edits to the document.
        public DateTime LastModified { get; set; }

        // Identifier for the user who last modified the document.
        // Tracks who made the latest changes to the document.
        public int ModifiedBy { get; set; }

        // Brief description of the document content or purpose.
        // Example: "Patient's lab results for annual checkup."
        public string Description { get; set; }
    }
}
