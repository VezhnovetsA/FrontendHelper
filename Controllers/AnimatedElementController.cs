using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class AnimatedElementController : Controller
    {
        private AnimatedElementRepository _animatedElementRepository;

        public AnimatedElementController(AnimatedElementRepository animatedElementRepository)
        {
            _animatedElementRepository = animatedElementRepository;
        }


        // показ всех иконок (без фильтра)
        public IActionResult ShowAllAnimatedElements()
        {
            var iconDatas = _animatedElementRepository.GetAssets();
            var viewModels = iconDatas.Select(PassDataToViewModel).ToList();
            return View(viewModels);
        }



        // показ всех иконок по определенной теме
        public IActionResult ShowAllAnimatedElementsOnTheTopic(string topic)
        {
            var viewModels = _animatedElementRepository
                .GetAllAnimatedElementsByTopic(topic)
                .Select(PassDataToViewModel)
                .ToList();

            ViewBag.Topic = topic;   // для вывода темы во вьюшке, ПОТОМ УБЕРУ
            return View(viewModels);
        }



        // показ групп иконок по всем темам
        public IActionResult ShowGroupsOfAnimatedElementsOnTheTopic(int numberOfIcons = 6)
        {
            var iconTopics = _animatedElementRepository.GetAnimatedElementTopics();

            var viewModels = iconTopics.Select(topic => new AnimatedElementGroupViewModel
            {
                Topic = topic,
                AnimatedElements = _animatedElementRepository
                .GetAnimatedElementGroupByTopic(topic, numberOfIcons)
                .Select(PassDataToViewModel)
                .ToList()
            });

            return View(viewModels);
        }
        private AnimatedElementViewModel PassDataToViewModel(AnimatedElementData elementData)
        {
            return new AnimatedElementViewModel
            {
                Id = elementData.Id,
                Name = elementData.Name,
                Img = elementData.Img,
                Topic = elementData.Topic
            };
        }
    }
}
