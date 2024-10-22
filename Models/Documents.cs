namespace iCareWebApplication.Models
{
    public class Documents
    {
        public int DocumentId { get; set; }
        public int PatientId { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }
        public int ModifiedBy { get; set; }

    }
}
