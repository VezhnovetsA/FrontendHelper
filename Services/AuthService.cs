using FhEnums;
using NuGet.Common;

namespace FrontendHelper.Services
{
    public class AuthService
    {
        public const string AUTH_TYPE = "Blebleble";
        public const string CLAIM_KEY_ID = "Id";
        public const string CLAIM_KEY_NAME = "Name";
        public const string CLAIM_KEY_PERMISSION = "Permission";

        private IHttpContextAccessor _contextAccessor;

        public AuthService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        internal string GetUserName()
        {
            var userName = GetClaim(CLAIM_KEY_NAME) ?? "Guest";

            return userName;
        }

        public int GetUserId()
        {
            var idString = GetClaim(CLAIM_KEY_ID);

            return int.Parse(idString);
        }

        public bool IsAuthenticated()
        {
            return _contextAccessor
                .HttpContext!
                .User
                ?.Identity
                ?.IsAuthenticated
                ??false;
        }

        public bool HasPermission(Permission permission)
        {
            var permissionInt = int.Parse(GetClaim(CLAIM_KEY_PERMISSION));
            if (permissionInt < 0) { return false; }
            var userPermission = (Permission)permissionInt;
            return userPermission.HasFlag(permission);
        }

        private string? GetClaim(string key)
        {
            return _contextAccessor
                 .HttpContext!
                 .User
                 .Claims
                 .FirstOrDefault(x => x.Type == key)
                 ?.Value;
        }

        public bool IsAdmin()
        {
            return GetUserName() == "admin";
        }
    }
}
