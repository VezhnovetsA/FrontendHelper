// FrontendHelper/Models/CreateAnimatedElementViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreateAnimatedElementViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Topic { get; set; }

        [Required]
        [FileExtensions(Extensions = "gif,mp4,webm", ErrorMessage = "Поддерживаются только GIF/MP4/WebM файлы.")]
        public IFormFile ImgFile { get; set; }
    }
}