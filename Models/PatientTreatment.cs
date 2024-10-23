namespace iCareWebApplication.Models
{
    public class PatientTreatment
    {
        public int PatientTreatmentId { get; set; }
        public int PatientId { get; set; }
        public int WorkerId { get; set; }
        public string Description { get; set; }
        public DateTime TreatmentDate { get; set; }
    }
}
