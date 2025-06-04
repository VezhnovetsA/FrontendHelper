using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreateIconViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Topic { get; set; }

        [Required]
        public IFormFile ImgFile { get; set; }

        /// <summary>
        /// Идентификаторы существующих фильтров, которые пользователь отметил.
        /// </summary>
        public List<int> SelectedFilterIds { get; set; } = new List<int>();

        /// <summary>
        /// Список всех существующих фильтров (AssetType="Icon"), чтобы отобразить их как чекбоксы.
        /// </summary>
        public List<SelectListItem> AvailableFilters { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Текстовая строка, куда пользователь может ввести несколько новых фильтров через запятую,
        /// например: "Красный, Большой, С подсветкой".
        /// </summary>
        [Display(Name = "Новые фильтры (через запятую)")]
        public string? NewFilterNames { get; set; }  // nullable — чтобы не валидировалось как обязательное
    }
}
