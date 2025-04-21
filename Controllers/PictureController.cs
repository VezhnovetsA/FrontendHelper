using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class PictureController : Controller
    {
        private PictureRepository _pictureRepository;

        public PictureController(PictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }



        // показ всех картинок на определенную тему
        public IActionResult ShowAllPicturesOnTheTopic(string topic)
        {
            var viewModels = _pictureRepository
                .GetAllPicturesByTopic(topic)
                .Select(PassDataToViewModel)
                .ToList();

            ViewBag.Topic = topic;   // для вывода темы во вьюшке, ПОТОМ УБЕРУ
            return View(viewModels);
        }




        // показать одну картинку по теме для превью группы
        public IActionResult ShowPreviewPictureOnTheTopic(string topic)
        {
            var pictureDatas = _pictureRepository.GetOnePicturePerTopic();

            var viewModels = pictureDatas
                .Where(x => x != null)
                .Select(PassDataToViewModel)
                .ToList();

            return View(viewModels);
        }




        // показ всех картинок (без фильтра)
        public IActionResult ShowAllPictures()
        {
            return View();
        }

        private PictureViewModel PassDataToViewModel(PictureData iconData)
        {
            return new PictureViewModel
            {
                Id = iconData.Id,
                Name = iconData.Name,
                Img = iconData.Img,
                Topic = iconData.Topic
            };
        }
    }
}
