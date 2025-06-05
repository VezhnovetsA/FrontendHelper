namespace FrontendHelper.Models
{
    public class FormViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Topic { get; set; }

        public string FormCode { get; set; }

        public bool IsFavorited { get; set; }

        public string FilterIdsCsv { get; set; }
    }
}
