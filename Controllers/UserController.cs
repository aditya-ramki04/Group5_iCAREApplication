using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly iCareContext _context;

        public UserController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Role/Index
        public async Task<IActionResult> Index()
        {
            var roles = await _context.User.ToListAsync();
            return View(roles);
        }
    }
}
