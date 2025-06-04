// FrontendHelper/Models/CreatePictureViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreatePictureViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Topic { get; set; }

        [Required]
        [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Поддерживаются только изображения (.jpg, .jpeg, .png, .gif).")]
        public IFormFile ImgFile { get; set; }
    }
}