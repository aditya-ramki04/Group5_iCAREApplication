using iCareWebApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iCareWebApplication.Controllers
{
    public class PatientTreatment : Controller
    {
        private readonly iCareContext _context;

        public PatientTreatment(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var roles = await _context.PatientTreatments.ToListAsync();
            return View(roles);
        }
    }
}

