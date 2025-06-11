using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditAnimatedElementViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Topic { get; set; }

        public string ExistingImg { get; set; }

        public IFormFile? ImgFile { get; set; }

        public List<int> SelectedFilterIds { get; set; } = new List<int>();
        public List<SelectListItem> AvailableFilters { get; set; } = new List<SelectListItem>();

        [Display(Name = "Новые фильтры (через запятую)")]
        public string? NewFilterNames { get; set; }
    }
}
