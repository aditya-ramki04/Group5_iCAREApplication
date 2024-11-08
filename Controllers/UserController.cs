using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    // UserController inherits from Controller to handle HTTP requests related to user management
    public class UserController : Controller
    {
        private readonly iCareContext _context;

        // Constructor: Initializes the UserController with a given iCareContext
        // Parameters:
        //   context (iCareContext): An instance of the database context to interact with the database
        // Returns:
        //   None
        public UserController(iCareContext context)
        {
            _context = context;
        }

        // Index Method: Handles the GET request for the Index page, fetching all users from the database
        // Parameters:
        //   None
        // Returns:
        //   IActionResult: A view containing a list of users fetched from the database
        public async Task<IActionResult> Index()
        {
            // Fetches all records of User from the database and stores them in a list
            var users = await _context.User.ToListAsync();

            // Returns the view with the fetched list of users as the model
            return View(users);
        }

        // Create Method (GET): Handles the GET request to render the user creation form
        // Parameters:
        //   None
        // Returns:
        //   IActionResult: A view that contains the form to create a new user
        public IActionResult Create()
        {
            return View();
        }

        // Create Method (POST): Handles the POST request to create a new user
        // Parameters:
        //   user (User): A model of the user to be created, populated with form data
        // Returns:
        //   IActionResult: Redirects to the Index page if the creation is successful, otherwise returns the creation form with validation errors
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            // Check if the user model is valid
            if (ModelState.IsValid)
            {
                // Set the creation date and account status for the new user
                user.DateCreate = DateTime.Now;
                user.AccountStatus = "Active";

                // Add the new user to the context (ready for insertion in the database)
                _context.Add(user);

                // Save the changes to the database
                await _context.SaveChangesAsync();

                // Redirect to the Index page after successful user creation
                return RedirectToAction(nameof(Index));
            }

            // Return the view with the current user model if there are validation errors
            return View(user);
        }

        // DeleteSelected Method (POST): Handles the POST request to delete multiple selected users
        // Parameters:
        //   selectedUserIds (int[]): An array of user IDs that the user wants to delete
        // Returns:
        //   IActionResult: Redirects to the Index page after successfully deleting the selected users
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedUserIds)
        {
            // Iterate through the selected user IDs to delete each user
            foreach (var userId in selectedUserIds)
            {
                // Find the user by their ID
                var user = await _context.User.FindAsync(userId);

                // If the user is found, remove them from the context
                if (user != null)
                {
                    _context.User.Remove(user);
                }
            }

            // Save all changes (deleting users) to the database
            await _context.SaveChangesAsync();

            // Redirect to the Index page after deleting the selected users
            return RedirectToAction(nameof(Index));
        }
    }
}
