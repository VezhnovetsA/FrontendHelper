using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditFontViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string FontFamily { get; set; }

        [Url]
        [MaxLength(300)]
        public string? Link { get; set; }

        public IFormFile? FontFile { get; set; }

        // Имя уже существующего локального файла
        public string? ExistingFileName { get; set; }

        // Список доступных фильтров (CheckboxList)
        public List<SelectListItem> AvailableFilters { get; set; } = new();

        // Список ID отмеченных фильтров
        public List<int> SelectedFilterIds { get; set; } = new();

        // Новые фильтры (через запятую)
        public string? NewFilterNames { get; set; }
    }
}
