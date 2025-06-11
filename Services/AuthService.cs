using FhEnums;
using static System.Net.WebRequestMethods;
using System.Security.Claims;
using FHDatabase.Repositories;

namespace FrontendHelper.Services
{
    public class AuthService
    {
        public const string AUTH_TYPE = "Blebleble";
        public const string CLAIM_KEY_ID = "Id";
        public const string CLAIM_KEY_NAME = "Name";
        public const string CLAIM_KEY_PERMISSION = "Permission";

        private IHttpContextAccessor _contextAccessor;
        private readonly RoleRepository _roles;

        public AuthService(IHttpContextAccessor contextAccessor, RoleRepository roleRepository)
        {
            _contextAccessor = contextAccessor;
            _roles = roleRepository;
        }

        
        public string GetUserName()
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

        public Permission CurrentPermission
        {
            get
            {
                if (!IsAuthenticated())
                {
                    var userRole = _roles.ByName("User");
                    return userRole?.Permission ?? 0;
                }

                if (Enum.TryParse(GetClaim(CLAIM_KEY_PERMISSION), out Permission p))
                    return p;

                return 0;
            }
        }

        //public Permission CurrentPermission =>
        //Enum.TryParse<Permission>(GetClaim(CLAIM_KEY_PERMISSION), out var p) ? p : 0;

        public bool HasPermission(Permission p) => CurrentPermission.HasFlag(p);

        public bool IsPremium() => HasPermission(Permission.CanManageFilters);

        //    public bool IsInRole(string roleName)
        //=> _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value == roleName;

        public bool IsInRole(string roleName)
    => _contextAccessor.HttpContext
        ?.User
        ?.IsInRole(roleName)
        ?? false;

        public bool IsPremiumUser() => IsInRole("PremiumUser");
        public bool IsUser() => IsInRole("User");

    }
}
