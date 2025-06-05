using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FrontendHelper.Models
{
    public class CreateButtonViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Topic { get; set; }

        [Required]
        public IFormFile HtmlFile { get; set; }
    }
}
