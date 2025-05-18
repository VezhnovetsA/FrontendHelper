using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Models
{

    public class ExtendedSelectListItem : SelectListItem
    {
        public IDictionary<string, string> DataAttributes { get; set; }
            = new Dictionary<string, string>();
    }

    public class FontViewModel
    {
        public string InputText { get; set; } = "Пример текста";
        public string SelectedFont { get; set; } = "";
        public List<ExtendedSelectListItem> AvailableFonts { get; set; }
        = new List<ExtendedSelectListItem>();

    }
}
