// FrontendHelper/Models/EditFormViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Имя уже существующего форм-файла
        public string ExistingCode { get; set; }

        [FileExtensions(Extensions = "html,htm", ErrorMessage = "Поддерживаются только HTML файлы (.html, .htm).")]
        public IFormFile? FormFile { get; set; }
    }
}