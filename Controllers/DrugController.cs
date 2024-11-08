using iCareWebApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class DrugController : Controller
    {
        private readonly iCareContext _context;

        /// Initializes a new instance of the DrugController class with the specified database context.
        ///
        /// Parameters:
        ///   context - The database context (iCareContext) used to access the application's data.
        public DrugController(iCareContext context)
        {
            _context = context;
        }

        /// Retrieves a list of all drugs from the database and displays it on the "Index" view.
        ///
        /// Returns:
        ///   The "Index" view with a list of all drugs retrieved from the database.
        public async Task<IActionResult> Index()
        {
            // Query the database for all drugs and return them as a list
            var drugs = await _context.Drugs.ToListAsync();

            // Return the "Index" view with the list of drugs
            return View(drugs);
        }
    }
}
