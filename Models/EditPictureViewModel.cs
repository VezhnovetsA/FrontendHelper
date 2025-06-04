// FrontendHelper/Models/EditPictureViewModel.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditPictureViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Topic { get; set; }

        // Имя уже существующего файла – нужно скрыто отправлять, если файл не меняется
        public string ExistingImg { get; set; }

        public IFormFile? ImgFile { get; set; }

        /// <summary>
        /// Уже привязанные фильтры (из базы).
        /// </summary>
        public List<int> SelectedFilterIds { get; set; } = new List<int>();

        /// <summary>
        /// Все доступные фильтры AssetType="Picture".
        /// </summary>
        public List<SelectListItem> AvailableFilters { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Новые фильтры через запятую (необязательно).
        /// </summary>
        [Display(Name = "Новые фильтры (через запятую)")]
        public string? NewFilterNames { get; set; }
    }
}