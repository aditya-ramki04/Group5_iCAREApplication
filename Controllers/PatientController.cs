using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
    {
        public class PatientController : Controller // Ensure this is RoleController and it inherits from Controller
        {
            private readonly iCareContext _context;

            public PatientController(iCareContext context)
            {
                _context = context;
            }

            // GET: /Role/Index
            public async Task<IActionResult> Index()
            {
                var patients = await _context.Patient.ToListAsync();
                return View(patients);
            }
        }
    }

