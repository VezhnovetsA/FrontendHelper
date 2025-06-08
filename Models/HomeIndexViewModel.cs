// FrontendHelper/Models/HomeIndexViewModel.cs
using System.Collections.Generic;

namespace FrontendHelper.Models
{
    public class HomeIndexViewModel
    {
        public string UserName { get; set; } = "";
        public List<SearchResultItem> Icons { get; set; } = new();
        public List<SearchResultItem> Pictures { get; set; } = new();
        public List<SearchResultItem> AnimatedElements { get; set; } = new();
        public List<SearchResultItem> Buttons { get; set; } = new();
        public List<SearchResultItem> Templates { get; set; } = new();
        public List<SearchResultItem> Forms { get; set; } = new();
        public List<SearchResultItem> Fonts { get; set; } = new();
        public List<SearchResultItem> Palettes { get; set; } = new();
    }
}
