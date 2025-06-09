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
        //private UserRepository _userRepository;

        //public AuthenticationController(UserRepository userRepository)
        //{
        //    _userRepository = userRepository;
        //}
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //public IActionResult Registration()
        //{
        //    return View();
        //}

        //public IActionResult Logout()
        //{
        //    HttpContext.SignOutAsync().Wait();
        //    return RedirectToAction("Index", "Home");
        //}

        //[HttpPost]
        //public IActionResult Login(AuthenticationViewModel authenticationViewModel)
        //{
        //    var user = _userRepository.Login(authenticationViewModel.UserName, authenticationViewModel.Password);

        //    if (user is null)
        //    {
        //        return RedirectToAction("Login");

        //    }

        //    var claims = new List<Claim>()
        //    {
        //        new Claim(AuthService.CLAIM_KEY_ID, user.Id.ToString()),
        //        new Claim(AuthService.CLAIM_KEY_NAME, user.UserName.ToString()),
        //        new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int?)user.Role?.Permission ?? -1).ToString()),
        //        new Claim(ClaimTypes.AuthenticationMethod, AuthService.AUTH_TYPE)
        //    };

        //    var identity = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
        //    var principal = new ClaimsPrincipal(identity);

        //    HttpContext.SignInAsync(principal).Wait();

        //    return RedirectToAction("Index", "Home");
        //}


        //[HttpPost]
        //public IActionResult Registration(AuthenticationViewModel authenticationViewModel)
        //{
        //    var isNameNotUniq = _userRepository.Any(authenticationViewModel.UserName);

        //    if (isNameNotUniq)
        //    {
        //        ModelState.AddModelError(nameof(AuthenticationViewModel.UserName), "Введенное имя неуникально");
        //    }


        //    if (!ModelState.IsValid)
        //    {
        //        return View(authenticationViewModel);
        //    }

        //    _userRepository.Registration(authenticationViewModel.UserName, authenticationViewModel.Password);

        //    return RedirectToAction("Login");
        //}

        private readonly UserRepository _users;

        public AuthenticationController(UserRepository users) => _users = users;

        public IActionResult Login() => View();
        public IActionResult Registration() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Registration(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                _users.Register(vm.Login, vm.Password);
                TempData["Msg"] = "Регистрация успешна, войдите.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel vm)
        {
            var user = _users.Login(vm.Login, vm.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин/пароль");
                return View(vm);
            }

            var claims = new[]
            {
            new Claim(AuthService.CLAIM_KEY_ID,  user.Id.ToString()),
            new Claim(AuthService.CLAIM_KEY_NAME,user.UserName),
            new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int)user.Role.Permission).ToString()),
            new Claim(ClaimTypes.AuthenticationMethod, AuthService.AUTH_TYPE),
            new Claim(ClaimTypes.Role, user.Role!.RoleName)
            };
            var id = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
            var pr = new ClaimsPrincipal(id);
            HttpContext.SignInAsync(pr).Wait();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }
    }
}
