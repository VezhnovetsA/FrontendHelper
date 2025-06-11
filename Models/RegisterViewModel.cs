using FrontendHelper.Models.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class RegisterViewModel
    {
        [Required, StringLength(100, MinimumLength = 4)]
        public string Login { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StrongPassword]
        public string Password { get; set; } = "";

        [Compare(nameof(Password)), DataType(DataType.Password)]
        public string RepeatPassword { get; set; } = "";
    }
}
