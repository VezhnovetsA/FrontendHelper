using FHDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;
using FrontendHelper.Models;
using Microsoft.EntityFrameworkCore;
using FrontendHelper.Services.Interfaces;
using FHDatabase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using FrontendHelper.Controllers.AuthorizationAttributes;

namespace FrontendHelper.Controllers
{
    public class IconController : Controller
    {
        private IconRepository _iconRepository;
        private readonly IFileService _fileService;
        private FilterRepository _filterRepository;
        public IconController(IconRepository iconRepository, IFileService fileService, FilterRepository filterRepository)
        {
            _iconRepository = iconRepository;
            _fileService = fileService;
            _filterRepository = filterRepository;
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




        //[HttpGet]
        //public IActionResult AddIcon() => View(new CreateIconViewModel());




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddIcon(CreateIconViewModel viewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(viewModel);

        //    if (_iconRepository.CheckIconDuplicate(viewModel.Name, viewModel.Topic))
        //    {
        //        ModelState.AddModelError(nameof(viewModel.Name),
        //            "Иконка с таким именем уже существует в этой теме");
        //        return View(viewModel);
        //    }

        //    // Сохраняем файл через сервис и получаем новое имя
        //    var savedFileName = await _fileService.SaveFileAsync(viewModel.ImgFile, "images/icons");

        //    // Добавляем запись в БД
        //    _iconRepository.AddAsset(new IconData
        //    {
        //        Name = viewModel.Name,
        //        Topic = viewModel.Topic,
        //        Img = savedFileName
        //    });

        //    return RedirectToAction(nameof(ShowGroupsOfIconsOnTheTopic));
        //}


        // ---------------------------------------------------------
        // GET: отдаём форму с доступными фильтрами
       
        [HttpGet]
        [Authorize]
        [HasPermission(FhEnums.Permission.CanAddPublicAsset)]
        public IActionResult AddIcon()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var viewModel = new CreateIconViewModel();

            viewModel.AvailableFilters = _filterRepository.GetAssets()
                .Where(f => f.AssetType == "Icon")            // только фильтры для иконок
                .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                .ToList();

            return View(viewModel);
        }

        // ---------------------------------------------------------
        // POST: сохраняем иконку + связи с фильтрами
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIcon(CreateIconViewModel vm)
        {
            // 1) Если валидация не прошла — вернуть форму с фильтрами
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 2) Проверка дубликата как было
            if (_iconRepository.CheckIconDuplicate(vm.Name, vm.Topic))
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "Иконка с таким именем уже существует в этой теме");

                vm.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();

                return View(vm);
            }

            // 3) Сохранение файла и создание записи IconData
            var savedFileName = await _fileService
                .SaveFileAsync(vm.ImgFile, "images/icons");

            var icon = new IconData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFileName
            };
            _iconRepository.AddAsset(icon);

            // 4) Связывание фильтров (AssetFilter) для каждого выбранного Id
            foreach (var filterId in vm.SelectedFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = filterId,
                    AssetType = "Icon",
                    AssetId = icon.Id
                });
            }

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
