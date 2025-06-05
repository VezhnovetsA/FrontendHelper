// ButtonController.cs
using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Controllers
{
    public class ButtonController : Controller
    {
        private readonly ButtonRepository _buttonRepository;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly IFileService _fileService;
        private readonly AuthService _authService;

        public ButtonController(
            ButtonRepository buttonRepository,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            IFileService fileService,
            AuthService authService
        )
        {
            _buttonRepository = buttonRepository;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _fileService = fileService;
            _authService = authService;
        }

        // ===========================
        // ПРОСМОТР ВСЕХ КНОПОК
        // ===========================
        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowAllButtons()
        {
            var allData = _buttonRepository.GetAssets();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vm = allData.Select(d => ToViewModel(d, userId)).ToList();
            return View(vm);
        }

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowAllButtonsOnTheTopic(string topic)
        {
            var data = _buttonRepository.GetButtonsByTopic(topic);
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vm = data.Select(d => ToViewModel(d, userId)).ToList();
            ViewBag.Topic = topic;
            return View(vm);
        }

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowGroupsOfButtons(int numberPerGroup = 6)
        {
            var topics = _buttonRepository.GetButtonTopics();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vm = topics.Select(t =>
            {
                var list = _buttonRepository
                    .GetButtonsByTopic(t)
                    .Take(numberPerGroup)
                    .Select(d => ToViewModel(d, userId))
                    .ToList();

                return new ButtonGroupViewModel
                {
                    Topic = t,
                    Buttons = list
                };
            }).ToList();

            return View(vm);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult CreateButton()
        {
            var vm = new CreateButtonViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Button")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CanManageButtons)]
        public async Task<IActionResult> CreateButton(CreateButtonViewModel vm)
        {
            // 1) Базовая валидация моделей (Name, Topic, HtmlFile)
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Button")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 2) Проверка расширения .html
            var ext = Path.GetExtension(vm.HtmlFile.FileName)?.ToLowerInvariant();
            if (ext != ".html")
            {
                ModelState.AddModelError(nameof(vm.HtmlFile), "Допускаются только HTML-файлы (.html).");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Button")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 3) Сохраняем файл и создаём ButtonData
            var savedFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "buttons");
            var newButton = new ButtonData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                ButtonCode = savedFileName
            };
            _buttonRepository.AddAsset(newButton);

            // 4) Обрабатываем "новые фильтры" из vm.NewFilterNames
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
                    .GetFiltersByCategory("Button")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Button" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // 5) Собираем итоговый список фильтров (чекбоксы + вновь созданные)
            var toBindFilterIds = vm.SelectedFilterIds
                                  .Concat(createdFilterIds)
                                  .Distinct()
                                  .ToList();

            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Button",
                    AssetId = newButton.Id
                });
            }

            // 6) Редирект на ShowAllButtonsOnTheTopic с явной передачей контроллера + topic
            return RedirectToAction(
                actionName: nameof(ShowAllButtonsOnTheTopic),
                controllerName: "Button",
                routeValues: new { topic = vm.Topic }
            );
        }



        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult EditButton(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            // Существующие фильтры
            var existingFilters = _filterRepository
                .GetFiltersForAsset("Button", id)
                .Select(f => f.Id)
                .ToList();

            var vm = new EditButtonViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingFileName = data.ButtonCode,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Button")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditButton(EditButtonViewModel vm)
        {
            // 1) ОБЯЗАТЕЛЬНО убираем из ModelState ExistingFileName, чтобы не было ошибки "поле обязательное"
            ModelState.Remove(nameof(vm.ExistingFileName));

            // 2) Если модель НЕ прошла валидацию (хотя бы одно поле нефайл-ориентированное невалидно),
            //    мы должны вернуть AvailableFilters, чтобы чекбоксы снова отрисовались
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Button")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var data = _buttonRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            // 3) Сохраняем старое имя файла, чтобы, если ниже ошибка, вернулось в форму
            vm.ExistingFileName = data.ButtonCode;

            // 4) Обновляем Name и Topic
            data.Name = vm.Name;
            data.Topic = vm.Topic;

            // 5) Если пришёл новый файл – проверяем расширение и меняем его
            if (vm.HtmlFile != null)
            {
                var ext = Path.GetExtension(vm.HtmlFile.FileName)?.ToLowerInvariant();
                if (ext != ".html")
                {
                    ModelState.AddModelError(nameof(vm.HtmlFile), "Допускаются только HTML-файлы (.html).");
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("Button")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }

                // удаляем старый и сохраняем новый
                _fileService.DeleteFile(data.ButtonCode, "buttons");
                var newFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "buttons");
                data.ButtonCode = newFileName;
            }
            // Если vm.HtmlFile == null, то data.ButtonCode останется прежним

            _buttonRepository.UpdateAsset(data);

            // ============================
            // Дальше обрабатываем фильтры (то же, что и в Create)
            // ============================

            // Собираем введённые новые фильтры
            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Button")
                    .FirstOrDefault(f => f.Name.Equals(fname, System.StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Button" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // Удаляем связи, которые пользователь снял (если он ранее выбирал фильтры)
            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Button", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Button", vm.Id, oldFid);
                }
            }

            // Собираем итоговый список (выбранные + новые) и добавляем те, которых не было
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
                        AssetType = "Button",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            return RedirectToAction(nameof(ShowAllButtons));
        }


        // ===========================
        // УДАЛЕНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteButton(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            // Удаляем физический файл
            _fileService.DeleteFile(data.ButtonCode, "buttons");

            // Снимаем связи с фильтрами
            var filterIds = _filterRepository
                .GetFiltersForAsset("Button", id)
                .Select(f => f.Id)
                .ToList();
            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("Button", id, fid);
            }

            // Удаляем саму запись
            _buttonRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllButtons));
        }

        // ===========================
        // СКАЧАТЬ КОД (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "buttons", data.ButtonCode);
            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            var contentBytes = System.IO.File.ReadAllBytes(physicalPath);
            var fileName = $"button_{data.Id}.html";
            return File(contentBytes, "text/html; charset=utf-8", fileName);
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================
        private ButtonViewModel ToViewModel(ButtonData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Button" && f.AssetId == d.Id);
            }

            // Собираем все связанные фильтры и склеиваем в CSV через запятую
            var filterIds = _filterRepository
                .GetFiltersForAsset("Button", d.Id)
                .Select(f => f.Id)
                .ToList();
            string csv = string.Join(",", filterIds);

            return new ButtonViewModel
            {
                Id = d.Id,
                Name = d.Name,
                ButtonCode = d.ButtonCode,
                Topic = d.Topic,
                IsFavorited = isFav,
                FilterIdsCsv = csv
            };
        }
    }
}
