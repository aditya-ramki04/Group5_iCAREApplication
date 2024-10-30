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
        public BoardController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Board/MyBoard
        public async Task<IActionResult> MyBoard()
        {
            // Retrieve the logged-in user's ID and role from the session
            int? workerId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var activePatients = await _context.PatientAssignment
                .Where(pa => pa.WorkerId == workerId && pa.Active)
                .Join(
                    _context.Patient,                      // Second table to join (Patients)
                    pa => pa.PatientId,                     // Key from PatientAssignments
                    p => p.PatientId,                       // Key from Patients
                    (pa, p) => p                            // Select the Patient entity
                )
                .ToListAsync();

            return View(activePatients);
        }



    }
}
