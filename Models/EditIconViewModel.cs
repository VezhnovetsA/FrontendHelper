using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditIconViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Topic { get; set; }

        // НЕ биндим это поле из формы
        [BindNever]
        public string ExistingImg { get; set; }

        // Файл может быть null
        public IFormFile? ImgFile { get; set; }

        // Список отмеченных ID фильтров
        public List<int> SelectedFilterIds { get; set; } = new List<int>();

        // НЕ биндим этот список из формы
        [BindNever]
        public List<SelectListItem> AvailableFilters { get; set; }
    }
}
