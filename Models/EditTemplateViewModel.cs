// FrontendHelper/Models/EditTemplateViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditTemplateViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Имя уже сохранённого HTML-файла
        public string ExistingCode { get; set; }

        [FileExtensions(Extensions = "html,htm", ErrorMessage = "Поддерживаются только HTML файлы (.html, .htm).")]
        public IFormFile? HtmlFile { get; set; }
    }
}