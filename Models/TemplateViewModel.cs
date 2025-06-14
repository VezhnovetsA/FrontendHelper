﻿namespace FrontendHelper.Models
{
    public class TemplateViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Topic { get; set; }

        public string TemplateCode { get; set; }

        public bool IsFavorited { get; set; }

        public string FilterIdsCsv { get; set; }
    }

}
