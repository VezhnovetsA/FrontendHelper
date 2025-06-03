using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class IconController : Controller
    {
        private readonly IconRepository _iconRepository;
        private readonly IFileService _fileService;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _authService;

        public IconController(
            IconRepository iconRepository,
            IFileService fileService,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            AuthService authService
        )
        {
            _iconRepository = iconRepository;
            _fileService = fileService;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _authService = authService;
        }

        // ===========================
        // ПРОСМОТР (CanViewIcons)
        // ===========================

        [HasPermission(Permission.CanViewIcons)]
        public IActionResult ShowAllIcons()
        {
            // (Не используется в данном запросе, но для единообразия можно сделать так же, как ShowAllIconsOnTheTopic)
            var all = _iconRepository.GetAssets();
            var userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            var viewModels = all
                .Select(d => ToViewModel(d, userId))
                .ToList();

            return View(viewModels);
        }

        [HasPermission(Permission.CanViewIcons)]
        public IActionResult ShowAllIconsOnTheTopic(string topic)
        {
            var data = _iconRepository.GetAllIconsByTopic(topic);
            var userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            var viewModels = data
                .Select(d => ToViewModel(d, userId))
                .ToList();

            ViewBag.Topic = topic;
            return View(viewModels);
        }

        [HasPermission(Permission.CanViewIcons)]
        public IActionResult ShowGroupsOfIconsOnTheTopic(int numberOfIcons = 6)
        {
            var topics = _iconRepository.GetIconTopics();
            var userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            var vm = topics.Select(topic => new IconGroupViewModel
            {
                Topic = topic,
                Icons = _iconRepository
                    .GetIconGroupByTopic(topic, numberOfIcons)
                    .Select(d => ToViewModel(d, userId))
                    .ToList()
            });

            return View(vm);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageIcons)
        // ===========================

        [HasPermission(Permission.CanManageIcons)]
        [HttpGet]
        public IActionResult CreateIcon()
        {
            var vm = new CreateIconViewModel
            {
                AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageIcons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIcon(CreateIconViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // Проверка на дубли
            if (_iconRepository.CheckIconDuplicate(vm.Name, vm.Topic))
            {
                ModelState.AddModelError(nameof(vm.Name), "Иконка с таким именем уже существует в этой теме");
                vm.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // Сохраняем файл
            var savedFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/icons");
            var newIcon = new IconData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFileName
            };
            _iconRepository.AddAsset(newIcon);

            // Привязываем фильтры
            foreach (var filterId in vm.SelectedFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = filterId,
                    AssetType = "Icon",
                    AssetId = newIcon.Id
                });
            }

            return RedirectToAction(nameof(ShowAllIcons));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageIcons)
        // ===========================

        [HasPermission(Permission.CanManageIcons)]
        [HttpGet]
        public IActionResult EditIcon(int id)
        {
            var iconData = _iconRepository.GetAsset(id);
            if (iconData == null)
                return NotFound();

            var existingFilters = _filterRepository
                .GetFiltersForAsset("Icon", id)
                .Select(f => f.Id)
                .ToList();

            var viewModel = new EditIconViewModel
            {
                Id = iconData.Id,
                Name = iconData.Name,
                Topic = iconData.Topic,
                ExistingImg = iconData.Img,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };

            return View(viewModel);
        }

        [HasPermission(Permission.CanManageIcons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIcon(EditIconViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableFilters = _filterRepository.GetAssets()
                    .Where(f => f.AssetType == "Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(viewModel);
            }

            var icon = _iconRepository.GetAsset(viewModel.Id);
            if (icon == null)
                return NotFound();

            // Обновляем поля
            icon.Name = viewModel.Name;
            icon.Topic = viewModel.Topic;

            // Если пришёл новый файл — сохраняем его и удаляем старый
            if (viewModel.ImgFile != null)
            {
                _fileService.DeleteFile(icon.Img, "images/icons");
                var newFileName = await _fileService.SaveFileAsync(viewModel.ImgFile, "images/icons");
                icon.Img = newFileName;
            }

            _iconRepository.UpdateAsset(icon);

            // === Обновляем связи с фильтрами ===
            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Icon", viewModel.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFilterId in currentFilterIds)
            {
                if (!viewModel.SelectedFilterIds.Contains(oldFilterId))
                {
                    _filterRepository.RemoveAssetFilter("Icon", viewModel.Id, oldFilterId);
                }
            }

            foreach (var newFilterId in viewModel.SelectedFilterIds)
            {
                if (!currentFilterIds.Contains(newFilterId))
                {
                    _filterRepository.AddAssetFilter(new AssetFilter
                    {
                        AssetType = "Icon",
                        AssetId = viewModel.Id,
                        FilterId = newFilterId
                    });
                }
            }

            return RedirectToAction(nameof(ShowAllIconsOnTheTopic), new { topic = icon.Topic });
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageIcons)
        // ===========================

        [HasPermission(Permission.CanManageIcons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteIcon(int id)
        {
            var icon = _iconRepository.GetAsset(id);
            if (icon == null) return NotFound();

            _fileService.DeleteFile(icon.Img, "images/icons");

            var filterIds = _filterRepository
                .GetFiltersForAsset("Icon", id)
                .Select(f => f.Id)
                .ToList();

            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("Icon", id, fid);
            }

            _iconRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowGroupsOfIconsOnTheTopic));
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        /// <summary>
        /// Конструирует IconViewModel и сразу проставляет флаг IsFavorited для текущего пользователя
        /// </summary>
        private IconViewModel ToViewModel(IconData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Icon" && f.AssetId == d.Id);
            }

            return new IconViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Topic = d.Topic,
                Img = d.Img,
                IsFavorited = isFav
            };
        }
    }
}
