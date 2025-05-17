using FrontendHelper.Models.CustomValidationAttributes;

namespace FrontendHelper.Models
{
    public class AuthenticationViewModel
    {
        public string UserName { get; set; }

        [AuthentificationPassword]
        public string Password { get; set; }
    }
}
