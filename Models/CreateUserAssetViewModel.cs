using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FrontendHelper.Models
{
    /// <summary>
    /// Модель для формы загрузки собственного ресурса пользователем.
    /// </summary>
    public class CreateUserAssetViewModel
    {
        [Required]
        [Display(Name = "Название ресурса")]
        public string Name { get; set; } = "";

        [Required]
        [Display(Name = "Тип ресурса")]
        public string ResourceType { get; set; } = "";

        [Display(Name = "Тема")]
        public string? Topic { get; set; }

        [Required]
        [Display(Name = "Файл")]
        public IFormFile File { get; set; }
    }
}
