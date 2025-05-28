namespace FrontendHelper.Models
{
    public class SearchResultItem
    {
        public string ResourceType { get; set; } = "";
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PreviewUrl { get; set; } = "";  // либо путь к картинке либо файл стилей
    }
}
