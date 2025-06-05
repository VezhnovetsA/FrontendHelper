using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendHelper.Models
{
    public class EditButtonViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Topic { get; set; }

        [BindNever]
        public string ExistingFileName { get; set; }

        public IFormFile? HtmlFile { get; set; }
    }
}
