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



        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowGroupsOfPicturesOnTheTopic(int numberOfPictures = 8)
        {
            var topics = _pictureRepository
                .GetPictureTopics()
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            var userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            var vm = topics.Select(topic =>
            {
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
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

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

            var savedFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
            var pictureData = new PictureData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFileName
            };
            _pictureRepository.AddAsset(pictureData);

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
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Picture")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var data = _pictureRepository.GetAsset(vm.Id);
            if (data == null)
                return NotFound();

            if (vm.ImgFile != null)
            {
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

                _fileService.DeleteFile(data.Img, "images/pictures");
                var newFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
                data.Img = newFileName;
            }

            data.Name = vm.Name;
            data.Topic = vm.Topic;

            _pictureRepository.UpdateAsset(data);

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


        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePicture(int id)
        {
            var data = _pictureRepository.GetAsset(id);
            if (data == null) return NotFound();

            _fileService.DeleteFile(data.Img, "images/pictures");

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
