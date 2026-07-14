using DoctorMobileApp.Models;
using Microsoft.AspNetCore.Mvc;
using MobieAppPatientFeedback.Models;
using System.Diagnostics;

namespace DoctorMobileApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(OpdFeedback));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult OpdFeedback(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid feedback link.");
            }
            // Pass token to the view if required
            ViewBag.Token = token;
            return View();
        }
    }
}