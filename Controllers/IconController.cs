using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Drawing;
using FhEnums;
namespace FrontendHelper.Controllers
{
    public class IconController : Controller
    {
        private IconRepository _iconRepository;
        private readonly IFileService _fileService;
        private FilterRepository _filterRepository;
        private FavoriteRepository _favoriteRepository;
        public IconController(IconRepository iconRepository,
            IFileService fileService,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository)
        {
            _iconRepository = iconRepository;
            _fileService = fileService;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
        }


        // показ всех иконок (без фильтра)
        public IActionResult ShowAllIcons()
        {
            var iconDatas = _iconRepository.GetAssets();
            var viewModels = iconDatas.Select(PassDataToViewModel).ToList();
            return View(viewModels);
        }



        public IActionResult ShowAllIconsOnTheTopic(string topic)
        {
            var userId = int.TryParse(User.FindFirst("Id")?.Value, out var uid) ? uid : (int?)null;

            var viewModels = _iconRepository
                .GetAllIconsByTopic(topic)
                .Select(data => PassDataToViewModelForFavorites(data, userId))
                .ToList();

            ViewBag.Topic = topic;
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
        [HasPermission(Permission.CanAddPublicAsset)]
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIcon(CreateIconViewModel viewModel)
        {
            //  валидация не прошла —> вернуть форму создания
            if (!ModelState.IsValid)
            {
                viewModel.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(viewModel);
            }

            //  проверка дубликата иконки (если нет дубликата - создание иконки)
            if (_iconRepository.CheckIconDuplicate(viewModel.Name, viewModel.Topic))
            {
                ModelState.AddModelError(nameof(viewModel.Name),
                    "Иконка с таким именем уже существует в этой теме");

                viewModel.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();

                return View(viewModel);
            }

            // сохранение картинки и создание записи в таблицу иконок
            var savedFileName = await _fileService
                .SaveFileAsync(viewModel.ImgFile, "images/icons");

            var icon = new IconData
            {
                Name = viewModel.Name,
                Topic = viewModel.Topic,
                Img = savedFileName
            };
            _iconRepository.AddAsset(icon);

            // связка выбранных фильтров (таблица AssetFilter) с иконкой по их id
            foreach (var filterId in viewModel.SelectedFilterIds)
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CanRemovePublicAsset)]
        public IActionResult DeleteIcon(int id)
        {
            var icon = _iconRepository.GetAsset(id);
            if (icon == null) return NotFound();
            _fileService.DeleteFile(icon.Img, "images/icons");
            _iconRepository.RemoveAsset(id);

            return RedirectToAction(nameof(ShowGroupsOfIconsOnTheTopic));
        }





        // передача данных во ViewModel (маппер)
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

        // еще один маппер но с работой с избранными
        private IconViewModel PassDataToViewModelForFavorites(IconData iconData, int? userId)
        {
            var isFav = userId.HasValue
                && _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Icon" && f.AssetId == iconData.Id);

            return new IconViewModel
            {
                Id = iconData.Id,
                Name = iconData.Name,
                Topic = iconData.Topic,
                Img = iconData.Img,
                IsFavorited = isFav
            };
        }

    }
}
