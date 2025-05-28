namespace FrontendHelper.Models
{
    public class SearchViewModel
    {
        public string Query { get; set; } = "";
        public string? ResourceType { get; set; } 
        public List<SearchResultItem> Results { get; set; } = new();
    }
}
