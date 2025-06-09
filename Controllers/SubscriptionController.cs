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
    public async Task<IActionResult> Buy(BuyPremiumViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Manage", new SubscriptionViewModel { IsPremium = false });

        // 1) Обновляем роль в базе
        var premiumRole = _roles.ByName("PremiumUser")!;
        _users.UpdateRole(_auth.GetUserId(), premiumRole.Id);

        // 2) Пересоздаём cookie с новыми клеймами
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

        TempData["Msg"] = "Подписка отменена, вы снова User.";
        return RedirectToAction(nameof(Manage));
    }

    /// <summary>
    /// Пересоздаёт ClaimsPrincipal в соответствии с текущей ролью пользователя
    /// </summary>
    private async Task ReSignInCurrentUserAsync()
    {
        // берём из базы пользователя вместе с ролью
        var user = _users.WithRoles()
                         .First(u => u.Id == _auth.GetUserId());

        // собираем новые клеймы
        var claims = new List<Claim> {
            new Claim(AuthService.CLAIM_KEY_ID, user.Id.ToString()),
            new Claim(AuthService.CLAIM_KEY_NAME, user.UserName),
            // теперь берём Permission из роли, уже обновлённой
            new Claim(AuthService.CLAIM_KEY_PERMISSION, ((int?)user.Role?.Permission ?? -1).ToString()),
            new Claim(ClaimTypes.AuthenticationMethod, AuthService.AUTH_TYPE)
        };
        var identity = new ClaimsIdentity(claims, AuthService.AUTH_TYPE);
        var principal = new ClaimsPrincipal(identity);

        // заменяем cookie
        await HttpContext.SignOutAsync(AuthService.AUTH_TYPE);
        await HttpContext.SignInAsync(principal);
    }
}
