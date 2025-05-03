using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrontendHelper.Controllers
{
    public class AuthenticationController : Controller
    {
        private UserRepository _userRepository;

        public AuthenticationController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Login(AuthenticationViewModel authenticationViewModel)
        {
            var user = _userRepository.Login(authenticationViewModel.UserName, authenticationViewModel.Password);

            if (user is null)
            {
                return RedirectToAction("Login");

            }

            var claims = new List<Claim>()
            {
                new Claim(AuthService.CLAIM_KEY_ID, user.Id.ToString()),
                new Claim(AuthService.CLAIM_KEY_NAME, user.UserName.ToString()),
                new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int?)user.Role?.Permission ?? -1).ToString()),
                new Claim(ClaimTypes.AuthenticationMethod, AuthService.AUTH_TYPE)
            };

            var identity = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(principal).Wait();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult Registration(AuthenticationViewModel authenticationViewModel)
        {
           _userRepository.Registration(authenticationViewModel.UserName, authenticationViewModel.Password);

            return RedirectToAction("Login");
        }
    }
}
