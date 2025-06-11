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

        public string? ExistingFileName { get; set; }

        public List<SelectListItem> AvailableFilters { get; set; } = new();

        public List<int> SelectedFilterIds { get; set; } = new();

        public string? NewFilterNames { get; set; }
    }
}
