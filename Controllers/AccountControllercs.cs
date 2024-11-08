using Microsoft.AspNetCore.Mvc;
using iCareWebApplication.Data;
using iCareWebApplication.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace iCareWebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly iCareContext _context;

        /// Initializes a new instance of the AccountController class with the specified database context.
        ///
        /// Parameters:
        ///   context - The database context (iCareContext) used to access the application's data.
        public AccountController(iCareContext context)
        {
            _context = context;
        }

        /// Displays the login view.
        /// 
        /// Returns:
        ///   The login view with the current user's RoleId stored in ViewData.
        [HttpGet]
        public IActionResult Login()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            ViewData["RoleId"] = roleId;
            return View("Login");
        }

        /// Authenticates the user based on the provided username and password.
        ///
        /// Parameters:
        ///   username - The username entered by the user.
        ///   password - The password entered by the user.
        ///
        /// Returns:
        ///   Redirects to the Home page if login is successful, or redisplays
        ///   the login view with an error message if login fails.
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Query database for user
            var user = _context.User.FirstOrDefault(u => u.UserName == username);

            // Validate password (use hashing for production)
            if (user != null && user.PasswordHash == password)
            {
                // Get the role name for the logged-in user
                var roleName = _context.Roles
                    .Where(r => r.RoleID == user.RoleID)
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                // Store user information in the session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetInt32("RoleId", user.RoleID);
                HttpContext.Session.SetString("Username", user.UserName);
                HttpContext.Session.SetString("Role", roleName);

                return RedirectToAction("Index", "Home");
            }

            // Display error message if login fails
            ViewData["Error"] = "Invalid username or password.";
            return View("Login");
        }

        /// Displays the registration view if the logged-in user has admin privileges.
        ///
        /// Returns:
        ///   The registration view if the user is an admin, or redirects to the
        ///   Home page otherwise.
        [HttpGet]
        public IActionResult Register()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            // Check if user is logged in and has admin privileges
            if (roleId == 1) // Admin role
            {
                return View("Register");
            }

            // Redirect non-admin users to the Home page
            return RedirectToAction("Index", "Home");
        }

        /// Registers a new user with the specified details, only accessible by admin users.
        ///
        /// Parameters:
        ///   fullName - The full name of the new user.
        ///   email - The email address of the new user.
        ///   username - The username for the new account.
        ///   password - The password for the new account.
        ///   employeeType - The role type for the new user, determining RoleID.
        ///
        /// Returns:
        ///   Redirects to the Home page upon successful registration, or displays
        ///   an error on the registration view if input is invalid.
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string username, string password, string employeeType)
        {
            // Check if the logged-in user is an admin
            int? currentRoleId = HttpContext.Session.GetInt32("RoleId");
            if (currentRoleId == null || currentRoleId != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            // Map EmployeeType to RoleID
            int roleId = employeeType switch
            {
                "Physician" => 2,
                "Nurse" => 3,
                "Receptionist" => 4,
                "Lab Technician" => 5,
                "Doctor" => 6,
                _ => 0 // Default to 0 if no match
            };

            // Validate EmployeeType
            if (roleId == 0)
            {
                ModelState.AddModelError("", "Invalid Employee Type.");
                return View("Register");
            }

            // Create a new User object with the provided data
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                UserName = username,
                PasswordHash = password, // Apply hashing for production
                RoleID = roleId,
                DateCreate = DateTime.Now,
                AccountStatus = "Active"
            };

            // Add the user to the database
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            // Redirect to Login page upon successful registration
            return RedirectToAction("Index", "Home");
        }
    }
}
