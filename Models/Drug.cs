namespace iCareWebApplication.Models
{
    public class Drug
    {
        public int DrugId { get; set; }
        public required string DrugName { get; set; }
        public required string Description { get; set; }
        public int ExternalId { get; set; }
    }
}
