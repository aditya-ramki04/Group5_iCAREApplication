namespace iCareWebApplication.Models
{
    // Represents a drug entity in the iCare web application, used to store information about different drugs.
    public class Drug
    {
        // Unique identifier for the Drug entity in the database.
        // This value is auto-incremented by the database.
        public int DrugId { get; set; }

        // Name of the drug.
        // Example: "Aspirin"
        public string DrugName { get; set; }

        // Brief description of the drug, detailing its use or other relevant information.
        // Example: "Pain reliever and anti-inflammatory"
        public string Description { get; set; }

        // An identifier used for external reference or integration with external systems.
        // This can be useful if the application needs to sync drug data with external sources.
        public int ExternalId { get; set; }
    }
}
