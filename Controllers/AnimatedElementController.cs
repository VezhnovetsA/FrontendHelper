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
    public class AnimatedElementController : Controller
    {
        private readonly AnimatedElementRepository _animatedElementRepository;
        private readonly IFileService _fileService;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _auth_service;

        public AnimatedElementController(
            AnimatedElementRepository animatedElementRepository,
            IFileService fileService,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            AuthService authService
        )
        {
            _animatedElementRepository = animatedElementRepository;
            _fileService = fileService;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _auth_service = authService;
        }

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowGroupsOfAnimatedElementsOnTheTopic(int numberOfElements = 8)
        {
            var topics = _animatedElementRepository.GetAnimatedElementTopics()
                         .Where(t => !string.IsNullOrEmpty(t))
                         .ToList();

            var userId = _auth_service.IsAuthenticated()
                         ? _auth_service.GetUserId()
                         : (int?)null;

            var vm = topics.Select(topic =>
            {
                var list = _animatedElementRepository
                    .GetAnimatedElementGroupByTopic(topic, numberOfElements)
                    .ToList();

                var viewModels = list
                    .Select(d => PassDataToViewModel(d, userId))
                    .ToList();

                return new AnimatedElementGroupViewModel
                {
                    Topic = topic,
                    AnimatedElements = viewModels
                };
            }).ToList();

            return View(vm);
        }

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowAllAnimatedElementsOnTheTopic(string topic)
        {
            var data = _animatedElementRepository.GetAllAnimatedElementsByTopic(topic);
            var userId = _auth_service.IsAuthenticated()
                         ? _auth_service.GetUserId()
                         : (int?)null;

            var vm = data.Select(d => PassDataToViewModel(d, userId)).ToList();
            ViewBag.Topic = topic;
            return View(vm);
        }

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowAllAnimatedElements()
        {
            var data = _animatedElementRepository.GetAssets();
            var userId = _auth_service.IsAuthenticated()
                         ? _auth_service.GetUserId()
                         : (int?)null;

            var vm = data.Select(d => PassDataToViewModel(d, userId)).ToList();
            return View(vm);
        }


        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpGet]
        public IActionResult CreateAnimatedElement()
        {
            var vm = new CreateAnimatedElementViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnimatedElement(CreateAnimatedElementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            if (vm.ImgFile != null)
            {
                var allowedExt = new[] { ".gif", ".mp4", ".webm" };
                var ext = Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                {
                    ModelState.AddModelError(nameof(vm.ImgFile), "Поддерживаются только файлы .gif, .mp4 или .webm.");
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("AnimatedElement")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }
            }
            else
            {
                ModelState.AddModelError(nameof(vm.ImgFile), "Пожалуйста, выберите файл.");
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var savedFile = await _fileService.SaveFileAsync(vm.ImgFile, "images/animated-elements");
            var entity = new AnimatedElementData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFile
            };
            _animatedElementRepository.AddAsset(entity);

            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .FirstOrDefault(f => f.Name.Equals(fname, System.StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "AnimatedElement" };
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
                    AssetType = "AnimatedElement",
                    AssetId = entity.Id
                });
            }

            return RedirectToAction(nameof(ShowAllAnimatedElementsOnTheTopic), new { topic = vm.Topic });
        }


        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpGet]
        public IActionResult EditAnimatedElement(int id)
        {
            var data = _animatedElementRepository.GetAsset(id);
            if (data == null) return NotFound();

            var existingFilters = _filterRepository
                .GetFiltersForAsset("AnimatedElement", id)
                .Select(f => f.Id)
                .ToList();

            var vm = new EditAnimatedElementViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingImg = data.Img,
                SelectedFilterIds = existingFilters,
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList(),
                NewFilterNames = null
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnimatedElement(EditAnimatedElementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var data = _animatedElementRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            if (vm.ImgFile != null)
            {
                var allowedExt = new[] { ".gif", ".mp4", ".webm" };
                var ext = Path.GetExtension(vm.ImgFile.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                {
                    ModelState.AddModelError(nameof(vm.ImgFile), "Поддерживаются только файлы .gif, .mp4 или .webm.");
                    vm.AvailableFilters = _filterRepository
                        .GetFiltersByCategory("AnimatedElement")
                        .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                        .ToList();
                    return View(vm);
                }

                _fileService.DeleteFile(data.Img, "images/animated-elements");
                var newName = await _fileService.SaveFileAsync(vm.ImgFile, "images/animated-elements");
                data.Img = newName;
            }

            data.Name = vm.Name;
            data.Topic = vm.Topic;
            _animatedElementRepository.UpdateAsset(data);

            var allNewFilterNames = (vm.NewFilterNames ?? "")
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var createdFilterIds = new List<int>();
            foreach (var fname in allNewFilterNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("AnimatedElement")
                    .FirstOrDefault(f => f.Name.Equals(fname, System.StringComparison.OrdinalIgnoreCase));
                if (already != null)
                {
                    createdFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = fname, AssetType = "AnimatedElement" };
                    _filterRepository.AddAsset(newFilter);
                    createdFilterIds.Add(newFilter.Id);
                }
            }

            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("AnimatedElement", vm.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!vm.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("AnimatedElement", vm.Id, oldFid);
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
                        AssetType = "AnimatedElement",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            return RedirectToAction(nameof(ShowAllAnimatedElementsOnTheTopic), new { topic = data.Topic });
        }

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAnimatedElement(int id)
        {
            var data = _animatedElementRepository.GetAsset(id);
            if (data == null) return NotFound();

            _fileService.DeleteFile(data.Img, "images/animated-elements");

            var filterIds = _filterRepository
                .GetFiltersForAsset("AnimatedElement", id)
                .Select(f => f.Id)
                .ToList();

            foreach (var fid in filterIds)
            {
                _filterRepository.RemoveAssetFilter("AnimatedElement", id, fid);
            }

            _animatedElementRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllAnimatedElements));
        }


        private AnimatedElementViewModel PassDataToViewModel(AnimatedElementData d, int? userId)
        {
            bool isFav = false;
            if (userId.HasValue)
            {
                isFav = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Any(f => f.AssetType == "AnimatedElement" && f.AssetId == d.Id);
            }

            return new AnimatedElementViewModel
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
