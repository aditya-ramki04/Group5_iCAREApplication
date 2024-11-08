using Microsoft.AspNetCore.Identity;

namespace iCareWebApplication.Models
{
    // Represents a user in the iCare web application.
    // The User class is used for managing user credentials, personal information, and account status.
    public class User
    {
        // Unique identifier for the User entity in the database.
        public int UserId { get; set; }

        // Username of the user, used for login and authentication.
        // Example: "john_doe"
        public string UserName { get; set; }

        // Hashed password of the user for secure storage.
        // Used for user authentication.
        public string PasswordHash { get; set; }

        // Full name of the user.
        // Example: "John Doe"
        public string FullName { get; set; }

        // Email address of the user.
        // Example: "john.doe@example.com"
        public string Email { get; set; }

        // The date and time when the user account was created.
        // Used for tracking account creation and for auditing purposes.
        public DateTime DateCreate { get; set; }

        // Status of the user account (e.g., "Active", "Inactive").
        // Helps to manage user access based on their account status.
        public string AccountStatus { get; set; }

        // Role ID associated with the user.
        // Defines the user’s role in the application (e.g., "Admin", "Physician").
        public int RoleID { get; set; }
    }
}
