using FrontendHelper.interfaces;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class Icon : IPictureTypeResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Topic { get; set; }

        [Required]
        [MaxLength(300)]
        public string Img { get; set; }
    }
}
