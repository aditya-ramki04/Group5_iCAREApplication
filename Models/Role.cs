namespace iCareWebApplication.Models
{
    // Represents a user role in the iCare web application.
    // Defines access levels and permissions associated with different roles.
    public class Role
    {
        // Unique identifier for the Role entity in the database.
        public int RoleID { get; set; }

        // Name of the role.
        // Example: "Physician", "Nurse", "Admin".
        public string RoleName { get; set; }

        // Permissions associated with the role.
        // Specifies the actions a user with this role can perform.
        // Example: "Read, Write, Update" for data access permissions.
        public string Permissions { get; set; }
    }
}
