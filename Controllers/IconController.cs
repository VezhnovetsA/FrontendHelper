using FHDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;
using FrontendHelper.Models;
using Microsoft.EntityFrameworkCore;
using FrontendHelper.Services.Interfaces;

namespace FrontendHelper.Controllers
{
    public class IconController : Controller
    {
        private IconRepository _iconRepository;
        private readonly IFileService _fileService;

        public IconController(IconRepository iconRepository, IFileService fileService)
        {
            _iconRepository = iconRepository;
            _fileService = fileService;
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

        [HttpGet]
        public IActionResult AddIcon() => View(new CreateIconViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIcon(CreateIconViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (_iconRepository.CheckIconDuplicate(viewModel.Name, viewModel.Topic))
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "Иконка с таким именем уже существует в этой теме");
                return View(viewModel);
            }

            // Сохраняем файл через сервис и получаем новое имя
            var savedFileName = await _fileService.SaveFileAsync(viewModel.ImgFile, "images/icons");

            // Добавляем запись в БД
            _iconRepository.AddAsset(new IconData
            {
                Name = viewModel.Name,
                Topic = viewModel.Topic,
                Img = savedFileName
            });

            return RedirectToAction(nameof(ShowGroupsOfIconsOnTheTopic));
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
