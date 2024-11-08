using Microsoft.AspNetCore.Mvc;
using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class BoardController : Controller
    {
        private readonly iCareContext _context;

        /// Initializes a new instance of the BoardController class with the specified database context.
        ///
        /// Parameters:
        ///   context - The database context (iCareContext) used to access the application's data.
        public BoardController(iCareContext context)
        {
            _context = context;
        }

        /// Retrieves a list of active patients assigned to the currently logged-in user and displays it on the "My Board" view.
        ///
        /// Returns:
        ///   The "My Board" view with a list of active patients assigned to the logged-in user.
        ///   If the user is not logged in, redirects to the Account/Login view.
        public async Task<IActionResult> MyBoard()
        {
            // Retrieve the logged-in user's ID and role from the session
            int? workerId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // Redirect to login if no user is logged in
            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Query the database for active patients assigned to the logged-in worker
            var activePatients = await _context.PatientAssignment
                .Where(pa => pa.WorkerId == workerId && pa.Active)
                .Join(
                    _context.Patient,                      // Second table to join (Patients)
                    pa => pa.PatientId,                    // Key from PatientAssignments
                    p => p.PatientId,                      // Key from Patients
                    (pa, p) => p                           // Select the Patient entity
                )
                .ToListAsync();

            // Return the "My Board" view with the list of active patients
            return View(activePatients);
        }
    }
}
