using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class PatientTreatmentController : Controller
    {
        private readonly iCareContext _context;

        public PatientTreatmentController(iCareContext context)
        {
            _context = context;
        }

        // GET: /PatientTreatment/Index/{patientId} - View all treatments for a specific patient
        public async Task<IActionResult> Index(int patientId)
        {
            // Retrieve all treatments for the specified patient
            var treatments = await _context.PatientTreatment
                .Where(t => t.PatientId == patientId)
                .ToListAsync();

            ViewBag.PatientId = patientId; // Pass the PatientId to the view for linking
            return View(treatments);
        }

        // GET: /PatientTreatment/Create/{patientId} - Display the form to create a new treatment for a specific patient
        public IActionResult Create(int patientId)
        {
            var treatment = new PatientTreatment
            {
                PatientId = patientId,
                TreatmentDate = DateTime.Now // Set default treatment date to today
            };
            return View(treatment);
        }

        // POST: /PatientTreatment/Create - Handle form submission to create a new treatment record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientTreatment treatment)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the worker ID from the session
                int? workerId = HttpContext.Session.GetInt32("UserId");
                if (workerId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                treatment.WorkerId = workerId.Value; // Set the worker ID
                _context.PatientTreatment.Add(treatment); // Add the new treatment record
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { patientId = treatment.PatientId });
            }
            return View(treatment);
        }
    }
}
