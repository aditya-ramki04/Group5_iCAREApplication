using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    // PaletteController inherits from Controller to handle HTTP requests related to palette data
    public class PaletteController : Controller
    {
        private readonly iCareContext _context;

        // Constructor: Initializes the PaletteController with a given iCareContext
        // Parameters:
        //   context (iCareContext): An instance of the database context to interact with the database
        // Returns:
        //   None
        public PaletteController(iCareContext context)
        {
            _context = context;
        }

        // Index Method: Handles the GET request for the Index page, fetching data from the database
        // Parameters:
        //   None
        // Returns:
        //   IActionResult: A view containing a list of documents (iCareDocuments) retrieved from the database
        public async Task<IActionResult> Index()
        {
            // Fetches all records of iCareDocuments from the database and stores them in a list
            var roles = await _context.iCareDocuments.ToListAsync();

            // Returns the view with the fetched list of documents as the model
            return View(roles);
        }
    }
}
