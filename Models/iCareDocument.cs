namespace iCareWebApplication.Models
{
    public class iCareDocument
    {
        public int iCareDocumentId { get; set; }
        public int PatientId { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }
        public int ModifiedBy { get; set; }
        public string Description { get; set; }

    }
}
