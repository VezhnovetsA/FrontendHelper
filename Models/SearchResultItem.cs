namespace FrontendHelper.Models
{
    public class SearchResultItem
    {
        public string ResourceType { get; set; } = "";
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Topic { get; set; }
        public string? PreviewUrl { get; set; }
        public string? CodeContent { get; set; }
        public string? DownloadUrl { get; set; }

        // Новые поля:
        // Для шрифтов: CSS-семейство
        public string? FontFamily { get; set; }

        // Для палитр: список цветов (Name + Hex)
        public List<PaletteColorViewModel>? PaletteColors { get; set; }
    }
}
