using FhEnums;

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
            var userName = GetClaim(CLAIM_KEY_NAME) ?? "Гость";

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
                ?? false;
        }

        public bool HasPermission(Permission permission)
        {
            // Если пользователь не аутентифицирован, сразу false
            if (!IsAuthenticated())
                return false;

            // Попробуем получить claim с правами
            var claimValue = GetClaim(CLAIM_KEY_PERMISSION);
            if (string.IsNullOrEmpty(claimValue))
                return false;

            // Если claim оказался непарсимым, тоже false
            if (!int.TryParse(claimValue, out var permissionInt))
                return false;

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
