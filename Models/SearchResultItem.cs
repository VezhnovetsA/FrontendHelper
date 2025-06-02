namespace FrontendHelper.Models
{
    public class SearchResultItem
    {
        // Тип ресурса: "Icon", "Picture", "AnimatedElement", "Button", "Template", "Form", "Font", "Palette"
        public string ResourceType { get; set; } = "";

        // ID ресурса в БД
        public int Id { get; set; }

        // Название (Name или Title)
        public string Name { get; set; } = "";

        // Тема (если применимо; для Button/Template/Form можно оставить пустым)
        public string? Topic { get; set; }

        // URL для превью: для изображений/иконок — ссылка на картинку; для кода — пустая строка
        public string? PreviewUrl { get; set; }

        // Содержимое кода (для Button/Form/Template), чтобы можно было показать <pre> в карточке
        public string? CodeContent { get; set; }

        // URL, по которому пользователь может скачать файл: 
        //  – для изображений/иконок — прямая ссылка на файл (или служебный экшен);
        //  – для Button/Form/Template — экшен, возвращающий файл .txt или .cs с кодом;
        //  – для Font/Palette можно также дать ссылку (у палитр это может быть, например, JSON+цвета)
        public string? DownloadUrl { get; set; }
    }
}
