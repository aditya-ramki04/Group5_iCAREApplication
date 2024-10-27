namespace iCareWebApplication.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public required string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public required string BloodGroup { get; set; }
        public required string BedId { get; set; }
        public required string TreatmentArea { get; set; }
        public int GeoCodeId { get; set; }


    }
}
