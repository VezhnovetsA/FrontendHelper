using FrontendHelper.Models.CustomValidationAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Models
{
    public class CreateIconViewModel
    {
        [RangeLength(2, 30)]
        public string Name { get; set; }

        [RangeLength(2, 30)]
        public string Topic { get; set; }
        public IFormFile ImgFile { get; set; }

        public List<SelectListItem> AvailableFilters { get; set; } = new();
        public List<int> SelectedFilterIds { get; set; } = new();
    }
}
