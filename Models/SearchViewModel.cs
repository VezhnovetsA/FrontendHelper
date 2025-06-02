namespace FrontendHelper.Models
{
    public class SearchViewModel
    {
        // Текстовый запрос
        public string Query { get; set; } = "";

        // Выбранный тип ресурса (например, "Icon", "Picture" и т.д.)
        public string? ResourceType { get; set; }

        // Список фильтров, доступных для текущего типа ресурса
        public List<FilterViewModel> AvailableFilters { get; set; } = new List<FilterViewModel>();

        // ID фильтров, которые пользователь отметил в форме
        public List<int> SelectedFilterIds { get; set; } = new List<int>();

        // Собственно результаты поиска
        public List<SearchResultItem> Results { get; set; } = new List<SearchResultItem>();
    }
}
