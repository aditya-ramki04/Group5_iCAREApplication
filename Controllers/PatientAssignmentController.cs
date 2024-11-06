using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class PatientAssignmentController : Controller
    {
        private readonly iCareContext _context;

        public PatientAssignmentController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var patientAssignments = await _context.PatientAssignment.ToListAsync();
            return View(patientAssignments);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPatients(int[] selectedPatients)
        {
            int? workerId = HttpContext.Session.GetInt32("UserId");

            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Process each selected patient
            foreach (var patientId in selectedPatients)
            {
                // Check if the patient is already assigned to this worker
                var existingAssignment = await _context.PatientAssignment
                    .FirstOrDefaultAsync(pa => pa.PatientId == patientId && pa.WorkerId == workerId && pa.Active);

                if (existingAssignment == null)
                {
                    // Create a new PatientAssignment record for this worker and patient
                    var newAssignment = new PatientAssignment
                    {
                        PatientId = patientId,
                        WorkerId = workerId.Value,
                        DateAssigned = DateTime.Now,
                        Active = true
                    };

                    _context.PatientAssignment.Add(newAssignment);
                }
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to the AssignablePatients page or MyBoard with success message
            return RedirectToAction("MyBoard", "Board");
        }


    }
}
