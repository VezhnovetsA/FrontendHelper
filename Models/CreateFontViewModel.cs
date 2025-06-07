using FrontendHelper.Models.CustomValidationAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IFormFile? FontFile { get; set; }

        // Список доступных фильтров (CheckboxList)
        public List<SelectListItem> AvailableFilters { get; set; } = new();

        // Список ID отмеченных фильтров
        public List<int> SelectedFilterIds { get; set; } = new();

        // Новые фильтры, вводимые строкой через запятую
        public string? NewFilterNames { get; set; }
    }
}
