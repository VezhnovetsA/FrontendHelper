using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class Button
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string ButtonCode { get; set; }

    }
}
