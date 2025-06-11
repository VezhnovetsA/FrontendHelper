namespace FrontendHelper.Models
{
    public class ButtonViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ButtonCode { get; set; }

        public string Topic { get; set; }

        public bool IsFavorited { get; set; }
        public string FilterIdsCsv { get; set; }
    }
}
