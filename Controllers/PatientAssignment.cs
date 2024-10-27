using iCareWebApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var roles = await _context.PatientAssignment.ToListAsync();
            return View(roles);
        }
    }
}
