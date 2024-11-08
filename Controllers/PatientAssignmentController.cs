using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    // PatientAssignmentController inherits from Controller to handle HTTP requests related to patient assignments
    public class PatientAssignmentController : Controller
    {
        private readonly iCareContext _context;

        // Constructor: Initializes the PatientAssignmentController with a given iCareContext
        // Parameters:
        //   context (iCareContext): An instance of the database context to interact with the database
        // Returns:
        //   None
        public PatientAssignmentController(iCareContext context)
        {
            _context = context;
        }

        // Index Method: Handles the GET request for the Index page, fetching all patient assignment records
        // Parameters:
        //   None
        // Returns:
        //   IActionResult: A view containing a list of patient assignments fetched from the database
        public async Task<IActionResult> Index()
        {
            // Fetches all records of PatientAssignment from the database and stores them in a list
            var patientAssignments = await _context.PatientAssignment.ToListAsync();

            // Returns the view with the fetched list of patient assignments as the model
            return View(patientAssignments);
        }

        // AssignPatients Method: Handles the POST request to assign selected patients to the worker
        // Parameters:
        //   selectedPatients (int[]): An array of patient IDs that the worker wants to assign
        // Returns:
        //   IActionResult: Redirects to the "MyBoard" action in the "Board" controller with a success message
        [HttpPost]
        public async Task<IActionResult> AssignPatients(int[] selectedPatients)
        {
            // Retrieves the worker's ID from the session
            int? workerId = HttpContext.Session.GetInt32("UserId");

            // If the worker is not logged in, redirects to the login page
            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Process each selected patient for assignment
            foreach (var patientId in selectedPatients)
            {
                // Check if the patient is already assigned to this worker and if the assignment is active
                var existingAssignment = await _context.PatientAssignment
                    .FirstOrDefaultAsync(pa => pa.PatientId == patientId && pa.WorkerId == workerId && pa.Active);

                if (existingAssignment == null)
                {
                    // Create a new PatientAssignment record if there is no existing active assignment
                    var newAssignment = new PatientAssignment
                    {
                        PatientId = patientId,
                        WorkerId = workerId.Value,
                        DateAssigned = DateTime.Now,
                        Active = true
                    };

                    // Adds the new assignment to the database context
                    _context.PatientAssignment.Add(newAssignment);
                }
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();

            // Redirects to the "MyBoard" action in the "Board" controller after successful assignment
            return RedirectToAction("MyBoard", "Board");
        }
    }
}
