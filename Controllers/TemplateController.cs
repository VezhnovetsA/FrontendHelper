// FrontendHelper/Controllers/TemplateController.cs
using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class TemplateController : Controller
    {
        private readonly TemplateRepository _templateRepository;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly IFileService _fileService;
        private readonly AuthService _authService;

        public TemplateController(
            TemplateRepository templateRepository,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            IFileService fileService,
            AuthService authService)
        {
            _templateRepository = templateRepository;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _fileService = fileService;
            _authService = authService;
        }

        // ===========================
        // ПРОСМОТР ВСЕХ ШАБЛОНОВ
        // ===========================

        [HasPermission(Permission.CanViewTemplates)]
        public IActionResult ShowAllTemplates()
        {
            var allData = _templateRepository.GetAssets();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmList = allData
                .Select(d => ToViewModel(d, userId))
                .ToList();

            return View(vmList);
        }

        [HasPermission(Permission.CanViewTemplates)]
        public IActionResult ShowAllTemplatesOnTheTopic(string topic)
        {
            var data = _templateRepository.GetTemplatesByTopic(topic);
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmList = data
                .Select(d => ToViewModel(d, userId))
                .ToList();

            ViewBag.Topic = topic;
            return View(vmList);
        }

        [HasPermission(Permission.CanViewTemplates)]
        public IActionResult ShowGroupsOfTemplates(int numberPerGroup = 6)
        {
            var topics = _templateRepository.GetTemplateTopics();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmGroups = topics.Select(t =>
            {
                var list = _templateRepository
                    .GetTemplatesByTopic(t)
                    .Take(numberPerGroup)
                    .Select(d => ToViewModel(d, userId))
                    .ToList();

                return new TemplateGroupViewModel
                {
                    Topic = t,
                    Templates = list
                };
            }).ToList();

            return View(vmGroups);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            var vm = new CreateTemplateViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Template")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate(CreateTemplateViewModel vm)
        {
            // 1) Базовая валидация
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Template")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 2) Проверка дубликата (имя + тема)
            if (_templateRepository.CheckTemplateDuplicate(vm.Name, vm.Topic))
            {
                ModelState.AddModelError(nameof(vm.Name), "Шаблон с таким именем и темой уже существует.");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Template")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 3) Сохраняем HTML-файл через IFileService
            var savedFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "templates");

            var newTemplate = new TemplateData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                TemplateCode = savedFileName
            };
            _templateRepository.AddAsset(newTemplate);

            // 4) Обрабатываем «новые фильтры»
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
                    .GetFiltersByCategory("Template")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Template" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // 5) Составляем итоговый список фильтров (чекбоксы + вновь созданные)
            var toBindFilterIds = vm.SelectedFilterIds
                                  .Concat(createdFilterIds)
                                  .Distinct()
                                  .ToList();

            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Template",
                    AssetId = newTemplate.Id
                });
            }

            // 6) Редирект на ShowAllTemplatesOnTheTopic
            return RedirectToAction(
                actionName: nameof(ShowAllTemplatesOnTheTopic),
                controllerName: "Template",
                routeValues: new { topic = vm.Topic }
            );
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult EditTemplate(int id)
        {
            var data = _templateRepository.GetTemplateById(id);
            if (data == null) return NotFound();

            var existingFilters = _filterRepository
                .GetFiltersForAsset("Template", id)
                .Select(f => f.Id)
                .ToList();

            var vm = new EditTemplateViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingCode = data.TemplateCode,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Template")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTemplate(EditTemplateViewModel vm)
        {
            // 1) Убираем из ModelState ExistingCode, чтобы не ругалось
            ModelState.Remove(nameof(vm.ExistingCode));

            // 2) Если модель невалидна — возвращаем AvailableFilters и обратно в форму
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Template")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var data = _templateRepository.GetTemplateById(vm.Id);
            if (data == null) return NotFound();

            // 3) Сохраняем старое имя файла, чтобы вернуть в форму в случае ошибки
            vm.ExistingCode = data.TemplateCode;

            // 4) Обновляем Name и Topic
            data.Name = vm.Name;
            data.Topic = vm.Topic;

            // 5) Если пришёл новый файл — проверяем расширение и заменяем
            if (vm.HtmlFile != null)
            {
                var ext = Path.GetExtension(vm.HtmlFile.FileName)?.ToLowerInvariant();
                if (ext != ".html" && ext != ".htm")
                {
                    ModelState.AddModelError(nameof(vm.HtmlFile), "Допускаются только HTML-файлы (.html, .htm).");
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("Template")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }

                // удаляем старый и сохраняем новый через IFileService
                _fileService.DeleteFile(data.TemplateCode, "templates");
                var newFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "templates");
                data.TemplateCode = newFileName;
            }
            // Если vm.HtmlFile == null, TemplateCode остаётся прежним

            _templateRepository.UpdateAsset(data);

            // ============================
            // Дальше обрабатываем фильтры (аналогично Create)
            // ============================
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
                    .GetFiltersByCategory("Template")
                    .FirstOrDefault(f => f.Name.Equals(fname, StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Template" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            // Удаляем связи, которые пользователь снял
            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Template", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Template", vm.Id, oldFid);
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
                        AssetType = "Template",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            return RedirectToAction(nameof(ShowGroupsOfTemplates));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageTemplates)
        // ===========================
        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTemplate(int id)
        {
            var data = _templateRepository.GetTemplateById(id);
            if (data == null) return NotFound();

            // Удаляем физический файл
            _fileService.DeleteFile(data.TemplateCode, "templates");

            // Снимаем связи с фильтрами
            var filterIds = _filterRepository
                .GetFiltersForAsset("Template", id)
                .Select(f => f.Id)
                .ToList();
            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("Template", id, fid);
            }

            // Удаляем запись
            _templateRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowGroupsOfTemplates));
        }

        // ===========================
        // СКАЧАТЬ КОД (CanManageTemplates)
        // ===========================
        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var data = _templateRepository.GetTemplateById(id);
            if (data == null) return NotFound();

            var content = System.IO.File.ReadAllText(Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "templates", data.TemplateCode));
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"template_{data.Id}.html";
            return File(bytes, "text/html; charset=utf-8", fileName);
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================
        private TemplateViewModel ToViewModel(TemplateData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Template" && f.AssetId == d.Id);
            }

            // Собираем все связанные фильтры и склеиваем в CSV через запятую
            var filterIds = _filterRepository
                .GetFiltersForAsset("Template", d.Id)
                .Select(f => f.Id)
                .ToList();
            string csv = string.Join(",", filterIds);

            return new TemplateViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Topic = d.Topic,
                TemplateCode = d.TemplateCode,
                IsFavorited = isFav,
                FilterIdsCsv = csv
            };
        }
    }
}
