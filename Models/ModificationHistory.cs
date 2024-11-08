namespace iCareWebApplication.Models
{
    // Represents the modification history for a document in the iCare web application.
    // Stores details of each modification made to a document, supporting audit tracking.
    public class ModificationHistory
    {
        // Unique identifier for the ModificationHistory record in the database.
        public int ModificationHistoryId { get; set; }

        // Identifier for the document that was modified.
        // Links this modification record to a specific document.
        public int DocumentId { get; set; }

        // Identifier for the user who made the modification.
        // Tracks which user was responsible for the change.
        public int ModifiedBy { get; set; }

        // Date and time when the modification was made.
        // Records when the change occurred.
        public DateTime ModificationDate { get; set; }

        // Description of the modification made.
        // Example: "Updated patient lab results section."
        public string ModificationDetails { get; set; }
    }
}
