using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class PaletteController : Controller // Ensure this is RoleController and it inherits from Controller
    {
        private readonly iCareContext _context;

        public PaletteController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var roles = await _context.iCareDocuments.ToListAsync();
            return View(roles);
        }
    }
}
