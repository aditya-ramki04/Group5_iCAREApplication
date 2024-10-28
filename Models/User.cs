using Microsoft.AspNetCore.Identity;

namespace iCareWebApplication.Models
{
    public class User
    {
        public int UserId {  get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime LastLogin { get; set; }
        public string AccountStatus { get; set; }
        public Role Role { get; set; }
    }
}
