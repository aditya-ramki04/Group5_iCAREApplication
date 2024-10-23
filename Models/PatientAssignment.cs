namespace iCareWebApplication.Models
{
    public class PatientAssignment
    {
        public int PatientAssignmentId {  get; set; }
        public int PatientId { get; set; }
        public int WorkerId { get; set; }
        public DateTime DateAssigned { get; set; }
        public bool Active { get; set; }

    }
}
