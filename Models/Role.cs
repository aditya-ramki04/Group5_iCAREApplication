namespace iCareWebApplication.Models
{
    public class Role
    {
        public int RoleID {  get; set; }
        public required string RoleName { get; set; }
        public required string Permissions { get; set; }
    }
}
