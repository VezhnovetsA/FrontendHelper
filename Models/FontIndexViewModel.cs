using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Models
{
    public class FontIndexViewModel
    {
        // Коллекция фильтров (ID + название)
        public List<SelectListItem> AvailableFilters { get; set; } = new();

        // Список шрифтов для таблицы
        public List<FontListItem> Fonts { get; set; } = new();

        // Текст, который вводит пользователь для предпросмотра
        public string InputText { get; set; } = "Пример текста";
    }

    public class FontListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FontFamily { get; set; }
        public string LinkOrLocalUrl { get; set; }  // либо внешняя ссылка, либо Url("~/fonts/xxx.ttf")
        public List<int> FilterIds { get; set; } = new();
        public bool IsFavorited { get; set; }
    }
}
