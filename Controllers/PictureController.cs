using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class PictureController : Controller
    {
        private readonly PictureRepository _pictureRepository;
        private readonly IFileService _fileService;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _authService;

        public PictureController(
            PictureRepository pictureRepository,
            IFileService fileService,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            AuthService authService
        )
        {
            _pictureRepository = pictureRepository;
            _fileService = fileService;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _authService = authService;
        }

        // ===========================
        // ПРОСМОТР (CanViewPictures)
        // ===========================


        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowGroupsOfPicturesOnTheTopic(int numberOfPictures = 8)
        {
            // получаем список всех тем картинок
            var topics = _pictureRepository
                .GetPictureTopics()
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            var userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            var vm = topics.Select(topic =>
            {
                // для каждой темы берём до numberOfPictures картинок
                var listOfPictures = _pictureRepository
                    .GetAllPicturesByTopic(topic)
                    .OrderBy(p => p.Id)
                    .Take(numberOfPictures)
                    .ToList();

                var pictureViewModels = listOfPictures
                    .Select(pic => PassDataToViewModel(pic, userId))
                    .ToList();

                return new PictureGroupViewModel
                {
                    Topic = topic,
                    Pictures = pictureViewModels
                };
            }).ToList();

            return View(vm);
        }


        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowAllPicturesOnTheTopic(string topic)
        {
            var data = _pictureRepository.GetAllPicturesByTopic(topic);
            var userId = _authService.IsAuthenticated()
                         ? _authService.GetUserId()
                         : (int?)null;

            var viewModels = data.Select(x => PassDataToViewModel(x, userId)).ToList();
            ViewBag.Topic = topic;
            return View(viewModels);
        }

        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowAllPictures()
        {
            var data = _pictureRepository.GetAssets();
            var userId = _authService.IsAuthenticated()
                         ? _authService.GetUserId()
                         : (int?)null;

            var vms = data.Select(x => PassDataToViewModel(x, userId)).ToList();
            return View(vms);
        }

        // ===========================
        // СОЗДАНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpGet]
        public IActionResult CreatePicture()
        {
            var vm = new CreatePictureViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePicture(CreatePictureViewModel vm)
        {
            // 1) Общая валидация полей
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 2) Достаем расширение загруженного файла и проверяем его “вручную”
            var extension = Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(extension))
            {
                ModelState.AddModelError(nameof(vm.ImgFile),
                    "Поддерживаются только файлы .jpg, .jpeg, .png");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 3) Сохраняем файл
            var savedFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
            var pictureData = new PictureData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFileName
            };
            _pictureRepository.AddAsset(pictureData);

            // 4) Обработка новых фильтров (если есть)
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
                    .GetFiltersByCategory("Picture")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Picture" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // 5) Собираем все выбранные фильтры (существующие + вновь созданные)
            var toBindFilterIds = vm.SelectedFilterIds
                                  .Concat(createdFilterIds)
                                  .Distinct()
                                  .ToList();
            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Picture",
                    AssetId = pictureData.Id
                });
            }

            return RedirectToAction(nameof(ShowAllPicturesOnTheTopic), new { topic = vm.Topic });
        }


        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpGet]
        public IActionResult EditPicture(int id)
        {
            var data = _pictureRepository.GetAsset(id);
            if (data == null) return NotFound();

            var existingFilters = _filterRepository
                .GetFiltersForAsset("Picture", id)
                .Select(f => f.Id)
                .ToList();

            var vm = new EditPictureViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingImg = data.Img,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPicture(EditPictureViewModel vm)
        {
            // Если изначально модель не прошла валидацию атрибутами (Name, Topic и т.п.), возвращаем форму
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // Получаем сущность из БД
            var data = _pictureRepository.GetAsset(vm.Id);
            if (data == null)
                return NotFound();

            // Если пользователь загрузил новый файл, проверяем его расширение
            if (vm.ImgFile != null)
            {
                var extension = Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                if (!allowed.Contains(extension))
                {
                    ModelState.AddModelError(nameof(vm.ImgFile),
                        "Поддерживаются только файлы .jpg, .jpeg, .png");
                    // Возвращаем AvailableFilters, чтобы форма корректно отобразилась
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("Picture")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }

                // Удаляем старый файл и сохраняем новый
                _fileService.DeleteFile(data.Img, "images/pictures");
                var newFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
                data.Img = newFileName;
            }
            // Если vm.ImgFile == null, оставляем data.Img без изменения

            // Обновляем остальные свойства
            data.Name = vm.Name;
            data.Topic = vm.Topic;

            _pictureRepository.UpdateAsset(data);

            // --- Обработка новых фильтров (если есть) ---
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
                    .GetFiltersByCategory("Picture")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Picture" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // --- Удаляем связи, которые пользователь снял в чекбоксах ---
            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Picture", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Picture", vm.Id, oldFid);
                }
            }

            // --- Добавляем связи для новых и оставшихся отмеченных фильтров ---
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
                        AssetType = "Picture",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            return RedirectToAction(nameof(ShowAllPicturesOnTheTopic), new { topic = data.Topic });
        }


        // ===========================
        // УДАЛЕНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePicture(int id)
        {
            var data = _pictureRepository.GetAsset(id);
            if (data == null) return NotFound();

            // Удаляем сам файл
            _fileService.DeleteFile(data.Img, "images/pictures");

            // Удаляем связи с фильтрами
            var filterIds = _filterRepository
                .GetFiltersForAsset("Picture", id)
                .Select(f => f.Id)
                .ToList();

            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("Picture", id, fid);
            }

            _pictureRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowGroupsOfPicturesOnTheTopic));
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private PictureViewModel PassDataToViewModel(PictureData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Picture" && f.AssetId == d.Id);
            }

            return new PictureViewModel
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
