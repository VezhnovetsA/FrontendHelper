namespace FrontendHelper.Models
{
    public class SearchViewModel
    {
        public string Query { get; set; } = "";

        public string? ResourceType { get; set; }

        public List<FilterViewModel> AvailableFilters { get; set; } = new List<FilterViewModel>();

        public List<int> SelectedFilterIds { get; set; } = new List<int>();

        public List<SearchResultItem> Results { get; set; } = new List<SearchResultItem>();
    }
}
