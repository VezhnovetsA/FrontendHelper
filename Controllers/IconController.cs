using FHDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;
using FrontendHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Controllers
{
    public class IconController : Controller
    {
        private IconRepository _iconRepository;

        public IconController(IconRepository iconRepository)
        {
            _iconRepository = iconRepository;
        }



        // показ всех иконок (без фильтра)
        public IActionResult ShowAllIcons()
        {
            var iconDatas = _iconRepository.GetAssets();
            var viewModels = iconDatas.Select(PassDataToViewModel).ToList();
            return View(viewModels);
        }



        // показ всех иконок по определенной теме
        public IActionResult ShowAllIconsOnTheTopic(string topic)
        {
            var viewModels = _iconRepository
                .GetAllIconsByTopic(topic)
                .Select(PassDataToViewModel)
                .ToList();              

            ViewBag.Topic = topic;   // для вывода темы во вьюшке, ПОТОМ УБЕРУ
            return View(viewModels);
        }



        // показ групп иконок по всем темам
        public IActionResult ShowGroupsOfIconsOnTheTopic(int numberOfIcons = 6)
        {
            var iconTopics = _iconRepository.GetIconTopics();

            var viewModels = iconTopics.Select(topic => new IconGroupViewModel
            {
                Topic = topic,
                Icons = _iconRepository
                .GetIconGroupByTopic(topic, numberOfIcons)
                .Select(PassDataToViewModel)
                .ToList()
            });

            return View(viewModels);
        }



        // показ иконок по введенному запросу
        public IActionResult ShowFoundIconsForRequest(string request)
        {
            return View();
        }



        // передача данных во ViewModel (return)
        private IconViewModel PassDataToViewModel(IconData iconData)
        {
            return new IconViewModel
            {
                Id = iconData.Id,
                Name = iconData.Name,
                Img = iconData.Img,
                Topic = iconData.Topic
            };
        }

    }
}
