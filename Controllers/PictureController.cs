using FrontendHelper.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class PictureController : Controller
    {

        private readonly IAllPictures pictureRepository;

        public PictureController(IAllPictures pictureRepository)
        {
            this.pictureRepository = pictureRepository;
        }

        public IActionResult PictureList()
        {
            var pictures = pictureRepository.Pictures;
            return View(pictures);
        }
    }
}
