using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreateIconViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Topic { get; set; }

        [Required]
        public IFormFile ImgFile { get; set; }
        public List<int> SelectedFilterIds { get; set; } = new List<int>();


        public List<SelectListItem> AvailableFilters { get; set; } = new List<SelectListItem>();

        [Display(Name = "Новые фильтры (через запятую)")]
        public string? NewFilterNames { get; set; }
    }
}
