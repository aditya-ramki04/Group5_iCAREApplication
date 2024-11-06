using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        // GET: User/Index
        public async Task<IActionResult> Index()
        {
            var users = await _context.User.ToListAsync();
            return View(users);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.DateCreate = DateTime.Now;
                user.AccountStatus = "Active";
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // POST: User/DeleteSelected
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedUserIds)
        {
            foreach (var userId in selectedUserIds)
            {
                var user = await _context.User.FindAsync(userId);
                if (user != null)
                {
                    _context.User.Remove(user);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
