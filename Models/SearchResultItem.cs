namespace FrontendHelper.Models
{
    public class SearchResultItem
    {
        public string ResourceType { get; set; } = "";
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Topic { get; set; }

        public string PreviewUrl { get; set; } = "";

        public string? DownloadUrl { get; set; }

        public string? FontFamily { get; set; }

        public List<SearchColorViewModel>? PaletteColors { get; set; } = new List<SearchColorViewModel>();

        public bool IsFavorited { get; set; } = false;
    }
}
