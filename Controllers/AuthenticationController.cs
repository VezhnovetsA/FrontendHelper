using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;

namespace FrontendHelper.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly UserRepository _users;

        public AuthenticationController(UserRepository users) => _users = users;

        public IActionResult Login() => View();
        public IActionResult Registration() => View();

       

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }




        [HttpPost]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = _users.Login(vm.Login, vm.Password);
            if (user == null)
                return View(vm);

            var claims = new List<Claim>
        {
            new Claim(AuthService.CLAIM_KEY_ID,         user.Id.ToString()),
            new Claim(AuthService.CLAIM_KEY_NAME,       user.UserName),
            new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int?)user.Role?.Permission ?? 0).ToString()),
            new Claim(ClaimTypes.Role,                  user.Role?.RoleName ?? ""),
            new Claim(ClaimTypes.AuthenticationMethod,  AuthService.AUTH_TYPE)
        };

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            var id = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
            var principal = new ClaimsPrincipal(id);

            HttpContext.SignInAsync(AuthService.AUTH_TYPE, principal, props).Wait();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Registration(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _users.Register(vm.Login, vm.Password);
            return RedirectToAction("Login");
        }

    }
}
