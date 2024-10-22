namespace iCareWebApplication.Models
{
    public class PatientTreatments
    {
        public int TreatmentId { get; set; }
        public int PatientId { get; set; }
        public int WorkerId { get; set; }
        public string Description { get; set; }
        public DateTime TreatmentDate { get; set; }
    }
}
