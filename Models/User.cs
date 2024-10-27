using Microsoft.AspNetCore.Identity;

namespace iCareWebApplication.Models
{
    public class User
    {
        public int UserId {  get; set; }
        public required string UserName { get; set; }
        public required string PasswordHash { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime LastLogin { get; set; }
        public required string AccountStatus { get; set; }
        public required Role Role { get; set; }
    }
}
