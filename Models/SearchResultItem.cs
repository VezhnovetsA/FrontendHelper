namespace FrontendHelper.Models
{
    public class SearchResultItem
    {
        public string ResourceType { get; set; } = "";
        public int Id { get; set; }
        public string Name { get; set; } = "";

        // для тех типов, у которых есть Topic (Icon, Picture, AnimatedElement)
        public string? Topic { get; set; }

        // URL для превью (для картинок, иконок, кнопок, форм и т. д.)
        public string PreviewUrl { get; set; } = "";

        // URL для скачивания: для шаблонов, форм, кнопок и т. д.
        public string? DownloadUrl { get; set; }

        // Для шрифтов: имя font-family (подставляется в style)
        public string? FontFamily { get; set; }

        // Для палитры: список цветов (Hex)
        public List<SearchColorViewModel>? PaletteColors { get; set; } = new List<SearchColorViewModel>();

        public bool IsFavorited { get; set; } = false;
    }
}
