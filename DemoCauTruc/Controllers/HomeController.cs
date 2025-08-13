using System.Diagnostics;
using DemoCauTruc.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoCauTruc.Controllers
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
            return View();
        }

      

    }
}
