// FrontendHelper/Models/EditPictureViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditPictureViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Topic { get; set; }

        // Имя уже существующего файла, чтобы показать превью
        public string ExistingImg { get; set; }

        [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Поддерживаются только изображения (.jpg, .jpeg, .png, .gif).")]
        public IFormFile? ImgFile { get; set; }
    }
}