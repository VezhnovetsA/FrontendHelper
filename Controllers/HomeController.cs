using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FrontendHelper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; //?
        private AuthService _authService;
        public HomeController(ILogger<HomeController> logger, AuthService authService)
        {
            _logger = logger; //?
            _authService = authService;
        }

        public IActionResult Index()
        {
            var username = _authService.GetUserName();

            var indexViewModel = new IndexViewModel
            {
                UserName = username
            };

            return View(indexViewModel);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}