using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly AuthService _auth;
    private readonly RoleRepository _roles;
    private readonly UserRepository _users;

    public SubscriptionController(AuthService a, RoleRepository r, UserRepository u)
        => (_auth, _roles, _users) = (a, r, u);

    public IActionResult Manage()
    {
        var vm = new SubscriptionViewModel
        {
            IsPremium = _auth.IsPremiumUser()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Buy(SubscriptionViewModel vm)
    {

        if (!ModelState.IsValid)
        {
            vm.IsPremium = false;
            return View("Manage", vm);
        }

        var premiumRole = _roles.ByName("PremiumUser")!;
        _users.UpdateRole(_auth.GetUserId(), premiumRole.Id);

        await ReSignInCurrentUserAsync();

        TempData["Msg"] = "Спасибо! Подписка оформлена. С карты ежемесячно будет списываться 0 BYN";
        return RedirectToAction(nameof(Manage));
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        var userRole = _roles.ByName("User")!;
        _users.UpdateRole(_auth.GetUserId(), userRole.Id);

        await ReSignInCurrentUserAsync();

        TempData["Msg"] = "Подписка отменена";
        return RedirectToAction(nameof(Manage));
    }


    private async Task ReSignInCurrentUserAsync()
    {

        var user = _users.WithRoles()
                         .First(u => u.Id == _auth.GetUserId());

        var claims = new List<Claim>
    {
        new Claim(AuthService.CLAIM_KEY_ID, user.Id.ToString()),
        new Claim(AuthService.CLAIM_KEY_NAME, user.UserName),
        new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int?)user.Role?.Permission ?? 0).ToString()),
        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? ""), 
        new Claim(ClaimTypes.AuthenticationMethod, AuthService.AUTH_TYPE)
    };

        var identity = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignOutAsync(AuthService.AUTH_TYPE);
        await HttpContext.SignInAsync(AuthService.AUTH_TYPE, principal);
    }

}
