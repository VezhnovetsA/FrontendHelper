using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models.CustomValidationAttributes
{
    public class AuthentificationPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var viewModel = (AuthenticationViewModel)validationContext.ObjectInstance;

            if (viewModel.Password == viewModel.UserName)
            {
                return new ValidationResult("Логин и пароль не должны быть одинаковыми");
            }

            return ValidationResult.Success;
        }
    }
}
