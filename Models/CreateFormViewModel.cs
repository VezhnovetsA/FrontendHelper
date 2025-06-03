// FrontendHelper/Models/CreateFormViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreateFormViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [FileExtensions(Extensions = "html,htm", ErrorMessage = "Поддерживаются только HTML файлы (.html, .htm).")]
        public IFormFile FormFile { get; set; }
    }
}