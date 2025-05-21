namespace FrontendHelper.Models
{
    public class TemplateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TemplateCode { get; set; }
        public string PreviewHtml { get; set; }  // для показа всех имеющихся шаблонов
        public string FullHtml { get; set; }  // для (возможного) просмотра деталей одного из них
    }

}
