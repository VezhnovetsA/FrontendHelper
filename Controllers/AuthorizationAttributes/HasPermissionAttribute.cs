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

            var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();

            if (authService.IsAdmin())
            {
                base.OnActionExecuting(context);
                return;
            }

            if (!authService.HasPermission(_requiredPermission))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
