// FrontendHelper/Models/CreateButtonViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class CreateButtonViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string ButtonCode { get; set; }
    }
}