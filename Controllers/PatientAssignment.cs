using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class PatientAssignment : Controller
    {
        private readonly iCareContext _context;

        public PatientAssignment(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var patientAssignments = await _context.PatientAssignment.ToListAsync();
            return View(patientAssignments);
        }
    }
}
