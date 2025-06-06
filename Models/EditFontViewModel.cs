using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditFontViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string FontFamily { get; set; }

        // Новой строкой добавляем свойство Link
        [Url]
        [MaxLength(300)]
        public string? Link { get; set; }


        public IFormFile? FontFile { get; set; }

        // Храним имя уже существующего файла (чтобы показать в представлении и/или не затерять его, если новый не пришёл)
        public string? ExistingFileName { get; set; }
    }
}
