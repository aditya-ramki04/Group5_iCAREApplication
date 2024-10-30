//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace iCareWebApplication.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }

//        // Login action
//        [HttpGet]
//        public IActionResult Login() => View();

//        [HttpPost]
//        public async Task<IActionResult> Login(LoginViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//                if (result.Succeeded)
//                {
//                    return RedirectToAction("Index", "Home");
//                }
//                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//            }
//            return View(model);
//        }

//        // Register action
//        [HttpGet]
//        public IActionResult Register() => View();

//        [HttpPost]
//        public async Task<IActionResult> Register(RegisterViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var result = await _userManager.CreateAsync(user, model.Password);

//                if (result.Succeeded)
//                {
//                    await _userManager.AddToRoleAsync(user, model.Role);
//                    await _signInManager.SignInAsync(user, isPersistent: false);
//                    return RedirectToAction("Index", "Home");
//                }
//                foreach (var error in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//            }
//            return View(model);
//        }

//        // Logout action
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Index", "Home");
//        }
//    }

//}

using Microsoft.AspNetCore.Mvc;
using iCareWebApplication.Data; // Ensure this matches the namespace in iCareContext
using iCareWebApplication.Models; // Ensure this matches the namespace for User model
using System.Linq;
using System.Threading.Tasks;

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
            return View("Login");
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Query database for user
            var user = _context.User.FirstOrDefault(u => u.UserName == username);

            // Validate password (use hashing for production)
            if (user != null && user.PasswordHash == password) // Replace with hash verification in real applications
            {
                // Redirect to Home page if login is successful
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
            return View("Register");
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string employeeType)
        {
            // Logic to add a new user to the database
            // This part requires a hashed password and other details
            return RedirectToAction("Login");
        }
    }
}


