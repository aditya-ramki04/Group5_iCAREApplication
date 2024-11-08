using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    // RoleController inherits from Controller to handle HTTP requests related to role management
    public class RoleController : Controller
    {
        private readonly iCareContext _context;

        // Constructor: Initializes the RoleController with a given iCareContext
        // Parameters:
        //   context (iCareContext): An instance of the database context to interact with the database
        // Returns:
        //   None
        public RoleController(iCareContext context)
        {
            _context = context;
        }

        // Index Method: Handles the GET request for the Index page, fetching all roles from the database
        // Parameters:
        //   None
        // Returns:
        //   IActionResult: A view containing a list of roles retrieved from the database
        public async Task<IActionResult> Index()
        {
            // Fetches all records of Roles from the database and stores them in a list
            var roles = await _context.Roles.ToListAsync();

            // Returns the view with the fetched list of roles as the model
            return View(roles);
        }
    }
}
