using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace iCareWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// Initializes a new instance of the HomeController class with the specified logger.
        ///
        /// Parameters:
        ///   logger - The logger (ILogger<HomeController>) used to log information, warnings, and errors for this controller.
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// Displays the main landing page of the application.
        ///
        /// Returns:
        ///   The "Index" view for the Home page.
        public IActionResult Index()
        {
            return View();
        }

        /// Displays the Privacy page of the application.
        ///
        /// Returns:
        ///   The "Privacy" view.
        public IActionResult Privacy()
        {
            return View();
        }

        /// Displays the error page with information about the current error, including a unique request ID.
        ///
        /// Returns:
        ///   The "Error" view with an ErrorViewModel that contains the Request ID for tracing the error.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
