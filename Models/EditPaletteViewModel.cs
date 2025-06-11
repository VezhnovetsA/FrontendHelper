using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditPaletteViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = "";

        public List<int> SelectedColorIds { get; set; } = new();
        public List<SelectListItem> AvailableColors { get; set; } = new();
        public List<PaletteColorViewModel>? NewColors { get; set; } = new();
        public List<int> SelectedFilterIds { get; set; } = new();
        public List<SelectListItem> AvailableFilters { get; set; } = new();

        [Display(Name = "Новые фильтры (через запятую)")]
        public string? NewFilterNames { get; set; } = "";
    }
}