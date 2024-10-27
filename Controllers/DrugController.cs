using iCareWebApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iCareWebApplication.Controllers
{
    public class DrugController : Controller
    {
       
            private readonly iCareContext _context;

            public DrugController(iCareContext context)
            {
                _context = context;
            }

            public async Task<IActionResult> Index()
            {
                var roles = await _context.Drugs.ToListAsync();
                return View(roles);
            }
    }
}
