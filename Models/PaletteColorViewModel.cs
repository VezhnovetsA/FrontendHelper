// FrontendHelper/Models/PaletteColorViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class PaletteColorViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [RegularExpression("^#([0-9A-Fa-f]{6})$", ErrorMessage = "HEX-код должен быть в формате #RRGGBB.")]
        public string Hex { get; set; }
    }
}