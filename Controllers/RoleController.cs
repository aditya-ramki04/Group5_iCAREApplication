using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class RoleController : Controller // Ensure this is RoleController and it inherits from Controller
    {
        private readonly iCareContext _context;

        public RoleController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }
    }
}
