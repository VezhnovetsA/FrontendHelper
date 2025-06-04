// FrontendHelper/Models/PictureGroupViewModel.cs
using System.Collections.Generic;

namespace FrontendHelper.Models
{
    public class PictureGroupViewModel
    {
        public string Topic { get; set; }
        public List<PictureViewModel> Pictures { get; set; }
    }
}