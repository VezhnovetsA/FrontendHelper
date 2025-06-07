using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Models
{
    public class FontIndexViewModel
    {
        public List<SelectListItem> AvailableFilters { get; set; } = new();
        public List<FontListItem> Fonts { get; set; } = new();
        public string InputText { get; set; } = "Пример текста";
    }

    public class FontListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FontFamily { get; set; }
        public string LinkOrLocalUrl { get; set; }
        public List<int> FilterIds { get; set; } = new();
        public bool IsFavorited { get; set; }
        public string CssFontFamily { get; set; }
    }
}
