using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models.CustomValidationAttributes
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return s.Length >= 8 &&
                   s.Any(char.IsUpper) &&
                   s.Any(char.IsLower) &&
                   s.Any(char.IsDigit);
        }

        public override string FormatErrorMessage(string n) =>
            "Пароль от 8 симв., с цифрой, строчн. и заглавн. буквой";
    }
}
