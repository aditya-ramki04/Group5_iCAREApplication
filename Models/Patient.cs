using System.ComponentModel.DataAnnotations;

namespace iCareWebApplication.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string BloodGroup { get; set; }
        public string BedId { get; set; }
        public string TreatmentArea { get; set; }

        [Range(1, 50, ErrorMessage = "GeoCodeId must be between 1 and 50.")]
        public int GeoCodeId { get; set; }

        public int DocumentCount { get; set; }


    }
}
