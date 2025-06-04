using FhEnums;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FrontendHelper.Controllers.AuthorizationAttributes
{
    public class HasPermissionAttribute : ActionFilterAttribute
    {
        private readonly Permission _requiredPermission;
        public HasPermissionAttribute(Permission requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Получаем AuthService из DI
            var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();

            // Если пользователь — “admin” (AuthService.IsAdmin()), пропускаем
            if (authService.IsAdmin())
            {
                base.OnActionExecuting(context);
                return;
            }

            // Иначе проверяем флаг в токене/клеймах
            if (!authService.HasPermission(_requiredPermission))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
