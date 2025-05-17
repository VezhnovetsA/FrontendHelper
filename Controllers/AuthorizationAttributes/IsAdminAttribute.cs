using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FhEnums;

namespace FrontendHelper.Controllers.AuthorizationAttributes
{
    public class IsAdminAttribute : ActionFilterAttribute
    {
        //private AuthService _authService;
        //private Permission _requiredPermission;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authService = context
                .HttpContext
                .RequestServices
                .GetRequiredService<AuthService>();

            if (!authService.IsAdmin())
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);

        }

    }
}
