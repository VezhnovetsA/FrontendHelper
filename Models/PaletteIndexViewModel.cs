using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Models
{

    public class PaletteIndexViewModel
    {
        public List<SelectListItem> AvailableFilters { get; set; } = new();
        public List<PaletteListItem> Palettes { get; set; } = new();
    }

    public class PaletteListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<string> ColorHexes { get; set; } = new();
        public List<int> FilterIds { get; set; } = new();
        public bool IsFavorited { get; set; }
    }

}
