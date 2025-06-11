using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class FormController : Controller
    {
        private readonly FormRepository _formRepository;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly IFileService _fileService;
        private readonly AuthService _authService;

        public FormController(
            FormRepository formRepository,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            IFileService fileService,
            AuthService authService)
        {
            _formRepository = formRepository;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _fileService = fileService;
            _authService = authService;
        }


        [HasPermission(Permission.CanViewForms)]
        public IActionResult ShowAllForms()
        {
            var allData = _formRepository.GetAssets();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmList = allData.Select(d => ToViewModel(d, userId)).ToList();
            return View(vmList);
        }

        [HasPermission(Permission.CanViewForms)]
        public IActionResult ShowAllFormsOnTheTopic(string topic)
        {
            var data = _formRepository.GetFormsByTopic(topic);
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmList = data.Select(d => ToViewModel(d, userId)).ToList();
            ViewBag.Topic = topic;
            return View(vmList);
        }

        [HasPermission(Permission.CanViewForms)]
        public IActionResult ShowGroupsOfForms(int numberPerGroup = 6)
        {
            var topics = _formRepository.GetFormTopics();
            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : (int?)null;

            var vmGroups = topics.Select(t =>
            {
                var list = _formRepository
                    .GetFormsByTopic(t)
                    .Take(numberPerGroup)
                    .Select(d => ToViewModel(d, userId))
                    .ToList();

                return new FormGroupViewModel
                {
                    Topic = t,
                    Forms = list
                };
            }).ToList();

            return View(vmGroups);
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult CreateForm()
        {
            var vm = new CreateFormViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Form")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permission.CanManageForms)]
        public async Task<IActionResult> CreateForm(CreateFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Form")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var ext = Path.GetExtension(vm.FormFile.FileName)?.ToLowerInvariant();
            if (ext != ".html")
            {
                ModelState.AddModelError(nameof(vm.FormFile), "Допускаются только HTML-файлы (.html).");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Form")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var savedFileName = await _fileService.SaveFileAsync(vm.FormFile, "forms");
            var newForm = new FormData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                FormCode = savedFileName
            };
            _formRepository.AddAsset(newForm);

            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new System.Collections.Generic.List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Form")
                    .FirstOrDefault(f => f.Name.Equals(fname, System.StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Form" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            var toBindFilterIds = vm.SelectedFilterIds
                                  .Concat(createdFilterIds)
                                  .Distinct()
                                  .ToList();

            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Form",
                    AssetId = newForm.Id
                });
            }

            return RedirectToAction(
                actionName: nameof(ShowAllFormsOnTheTopic),
                controllerName: "Form",
                routeValues: new { topic = vm.Topic }
            );
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult EditForm(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();

            var existingFilters = _filterRepository
                .GetFiltersForAsset("Form", id)
                .Select(f => f.Id)
                .ToList();

            var vm = new EditFormViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingFileName = data.FormCode,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Form")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForm(EditFormViewModel vm)
        {
            ModelState.Remove(nameof(vm.ExistingFileName));

            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Form")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var data = _formRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            vm.ExistingFileName = data.FormCode;

            data.Name = vm.Name;
            data.Topic = vm.Topic;

            if (vm.FormFile != null)
            {
                var ext = Path.GetExtension(vm.FormFile.FileName)?.ToLowerInvariant();
                if (ext != ".html")
                {
                    ModelState.AddModelError(nameof(vm.FormFile), "Допускаются только HTML-файлы (.html).");
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("Form")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }

                _fileService.DeleteFile(data.FormCode, "forms");
                var newFileName = await _fileService.SaveFileAsync(vm.FormFile, "forms");
                data.FormCode = newFileName;
            }

            _formRepository.UpdateAsset(data);

            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new System.Collections.Generic.List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Form")
                    .FirstOrDefault(f => f.Name.Equals(fname, System.StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "Form" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Form", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Form", vm.Id, oldFid);
                }
            }

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
                        AssetType = "Form",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            return RedirectToAction(nameof(ShowAllForms));
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteForm(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();


            _fileService.DeleteFile(data.FormCode, "forms");


            var filterIds = _filterRepository
                .GetFiltersForAsset("Form", id)
                .Select(f => f.Id)
                .ToList();
            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("Form", id, fid);
            }


            _formRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllForms));
        }


        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forms", data.FormCode);
            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            var contentBytes = System.IO.File.ReadAllBytes(physicalPath);
            var fileName = $"form_{data.Id}.html";
            return File(contentBytes, "text/html; charset=utf-8", fileName);
        }


        private FormViewModel ToViewModel(FormData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "Form" && f.AssetId == d.Id);
            }


            var filterIds = _filterRepository
                .GetFiltersForAsset("Form", d.Id)
                .Select(f => f.Id)
                .ToList();
            string csv = string.Join(",", filterIds);

            return new FormViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Topic = d.Topic,
                FormCode = d.FormCode,
                IsFavorited = isFav,
                FilterIdsCsv = csv
            };
        }
    }
}
