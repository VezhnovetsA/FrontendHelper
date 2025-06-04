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
        public IActionResult ShowGroupsOfIconsOnTheTopic(int numberOfIcons = 8)
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
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Icon")
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
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // Проверка на дубли...
            if (_iconRepository.CheckIconDuplicate(vm.Name, vm.Topic))
            {
                ModelState.AddModelError(nameof(vm.Name), "Иконка с таким именем уже существует в этой теме");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // Сохраняем файл
            var savedFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/icons");
            var newIcon = new IconData { Name = vm.Name, Topic = vm.Topic, Img = savedFileName };
            _iconRepository.AddAsset(newIcon);

            // 1) Найдём все новые имена из vm.NewFilterNames (если строка НЕ пустая)
            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new List<int>();
            foreach (var fname in allNewFilterNames)
            {
                // проверим, нет ли уже такого фильтра
                var already = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));

                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Icon" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // 2) Соберём все фильтры: уже выбранные + только что созданные
            var toBindFilterIds = vm.SelectedFilterIds
                                  .Concat(createdFilterIds)
                                  .Distinct()
                                  .ToList();

            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Icon",
                    AssetId = newIcon.Id
                });
            }

            return RedirectToAction(nameof(ShowAllIconsOnTheTopic), new { topic = vm.Topic });
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

            var vm = new EditIconViewModel
            {
                Id = iconData.Id,
                Name = iconData.Name,
                Topic = iconData.Topic,
                ExistingImg = iconData.Img,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };

            return View(vm);
        }

        [HasPermission(Permission.CanManageIcons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIcon(EditIconViewModel vm)
        {
            // Если модель не прошла валидацию, надо восстановить AvailableFilters
            // (и ExistingImg уже придёт из скрытого поля)
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var icon = _iconRepository.GetAsset(vm.Id);
            if (icon == null)
                return NotFound();

            // Обновляем Name и Topic
            icon.Name = vm.Name;
            icon.Topic = vm.Topic;

            // Если пришёл новый файл, то заменяем старый
            if (vm.ImgFile != null)
            {
                // Удаляем физически старый и сохраняем новый
                _fileService.DeleteFile(icon.Img, "images/icons");
                var newFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/icons");
                icon.Img = newFileName;
            }
            // Если vm.ImgFile == null, то icon.Img остаётся прежним, а vm.ExistingImg в форме
            // просто дублирует старое значение.

            _iconRepository.UpdateAsset(icon);

            // Обработка новых фильтров (как в CreateIcon)
            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Icon")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Icon" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // 1) Удаляем те связи, которые пользователь снял в чекбоксах
            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Icon", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Icon", vm.Id, oldFid);
                }
            }

            // 2) Собираем окончательный список: уже выбранные + вновь созданные
            var finalFilterIds = vm.SelectedFilterIds
                                 .Concat(createdFilterIds)
                                 .Distinct()
                                 .ToList();

            foreach (var fid in finalFilterIds)
            {
                if (!currentFilterIds.Contains(fid))
                {
                    _filterRepository.AddAssetFilter(new AssetFilter
                    {
                        AssetType = "Icon",
                        AssetId = vm.Id,
                        FilterId = fid
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
