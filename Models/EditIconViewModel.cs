using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditIconViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Topic { get; set; }

        public string ExistingImg { get; set; }

        public IFormFile? ImgFile { get; set; }

        public List<int> SelectedFilterIds { get; set; } = new List<int>();
        public List<SelectListItem> AvailableFilters { get; set; }
    }

}
