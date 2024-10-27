namespace iCareWebApplication.Models
{
    public class GeoCode
    {
        public int GeoCodeId { get; set; }
        public required string LocationName { get; set; }
        public required string Description { get; set; }
    }
}
