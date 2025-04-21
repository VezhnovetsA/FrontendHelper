using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models
{
    public class FontViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FontFamily { get; set; }

        public string Link { get; set; }
    }
}
