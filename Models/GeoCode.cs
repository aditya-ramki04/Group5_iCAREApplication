namespace iCareWebApplication.Models
{
    // Represents a geographic location entity used in the iCare web application, 
    // which could help categorize or filter patients by location.
    public class GeoCode
    {
        // Unique identifier for the GeoCode entity in the database.
        // This value is typically auto-incremented by the database.
        public int GeoCodeId { get; set; }

        // Name of the geographic location.
        // Example: "North Clinic"
        public string LocationName { get; set; }

        // Detailed description of the location, potentially including specific attributes or features.
        // Example: "Primary care facility located in the northern part of the city."
        public string Description { get; set; }
    }
}
