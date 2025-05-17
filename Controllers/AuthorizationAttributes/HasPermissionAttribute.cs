using FhEnums;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FrontendHelper.Controllers.AuthorizationAttributes
{
    public class HasPermissionAttribute : ActionFilterAttribute
    {
        private AuthService _authService;
        private Permission _requiredPermission;

        public HasPermissionAttribute(Permission requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _authService = context
                .HttpContext
                .RequestServices
                .GetRequiredService<AuthService>();

            if (!_authService.HasPermission(_requiredPermission))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);

        }

    }
}
