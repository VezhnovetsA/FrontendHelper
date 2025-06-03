// FrontendHelper/Models/EditPaletteViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class EditPaletteViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MinLength(1, ErrorMessage = "Нужно указать хотя бы один цвет.")]
        public List<PaletteColorViewModel> Colors { get; set; } = new();
    }
}