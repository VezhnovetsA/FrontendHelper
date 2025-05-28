using FrontendHelper.Models.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    [OnlyOneFontResource(nameof(Link), nameof(FontFile), ErrorMessage = "Укажите либо ссылку, либо файл шрифта.")]
    public class CreateFontViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string FontFamily { get; set; }

        [Url]
        public string? Link { get; set; }

        [FileExtensions(Extensions = "ttf,otf,woff,woff2", ErrorMessage = "Неподдерживаемый формат шрифта.")]
        public IFormFile? FontFile { get; set; }
    }
}
