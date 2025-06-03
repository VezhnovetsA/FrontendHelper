// FrontendHelper/Models/EditAnimatedElementViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditAnimatedElementViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Topic { get; set; }

        // Существующий файл (GIF/MP4/WebM)
        public string ExistingImg { get; set; }

        [FileExtensions(Extensions = "gif,mp4,webm", ErrorMessage = "Поддерживаются только GIF/MP4/WebM файлы.")]
        public IFormFile? ImgFile { get; set; }
    }
}