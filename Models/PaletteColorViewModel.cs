using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class PaletteColorViewModel
    {
        public int? Id { get; set; }           // для уже существующих цветов
        [MaxLength(7)]
        [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "Неверный формат HEX")]
        public string Hex { get; set; }        // для новых или существующих
    }
}