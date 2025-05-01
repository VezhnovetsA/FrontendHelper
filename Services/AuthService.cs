using NuGet.Common;

namespace FrontendHelper.Services
{
    public class AuthService
    {
        public const string AUTH_TYPE = "Blebleble";
        public const string CLAIM_KEY_ID = "Id";
        public const string CLAIM_KEY_NAME = "Name";

        private IHttpContextAccessor _contextAccessor;

        public AuthService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        internal string GetUserName()
        {
            var userName = _contextAccessor
                 .HttpContext!
                 .User
                 .Claims
                 .FirstOrDefault(x => x.Type == CLAIM_KEY_NAME)
                 ?.Value ?? "Guest";

            return userName;
        }

        public int GetUserId()
        {
            var idString = _contextAccessor
                 .HttpContext!
                 .User
                 .Claims
                 .FirstOrDefault(x => x.Type == CLAIM_KEY_ID)
                 .Value;

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
    }
}
