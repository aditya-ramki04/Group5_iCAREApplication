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

        public AccountController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            ViewData["RoleId"] = roleId;
            return View("Login");
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Query database for user
            var user = _context.User.FirstOrDefault(u => u.UserName == username);

            // Validate password (use hashing for production)
            if (user != null && user.PasswordHash == password) 
            {
                // Redirect to Home page if login is successful
                var roleName = _context.Roles
                .Where(r => r.RoleID == user.RoleID)
                .Select(r => r.RoleName)
                .FirstOrDefault();

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

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            // Check if user is logged in and has admin privileges
            if (roleId == 1) // Admin role
            {
                return View("Register"); // Show the registration view if user is an admin
            }

            // Redirect non-admin users to Home page
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string username, string password, string employeeType)
        {

            // Check if the logged-in user is an admin
            int? currentRoleId = HttpContext.Session.GetInt32("RoleId");
            if (currentRoleId == null || currentRoleId != 1)
            {
                // Redirect non-admin users to the home page or another appropriate page
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


