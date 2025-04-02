using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class Font
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string FontFamily { get; set; }

        [Required]
        [Url]
        [MaxLength(300)]
        public string Link { get; set; }
    }
}
