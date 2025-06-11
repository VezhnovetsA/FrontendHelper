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
    public class FontController : Controller
    {
        private readonly FontRepository _fontRepository;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _authService;
        private readonly IFileService _fileService;

        public FontController(
            FontRepository fontRepository,
            FilterRepository filterRepository,
            FavoriteRepository favoriteRepository,
            AuthService authService,
            IFileService fileService)
        {
            _fontRepository = fontRepository;
            _filterRepository = filterRepository;
            _favoriteRepository = favoriteRepository;
            _authService = authService;
            _fileService = fileService;
        }

        [HasPermission(Permission.CanViewFonts)]
        public IActionResult ShowFonts()
        {
            var fonts = _fontRepository.GetAssets().ToList();
            var allFilters = _filterRepository.GetFiltersByCategory("Font").ToList();

            var filterSelectItems = allFilters
                .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                .ToList();

            int? userId = _authService.IsAuthenticated() ? _authService.GetUserId() : null;
            var userFavFontIds = new List<int>();
            if (userId.HasValue)
            {
                userFavFontIds = _favoriteRepository
                    .GetFavoriteElementByUser(userId.Value)
                    .Where(x => x.AssetType == "Font")
                    .Select(x => x.AssetId)
                    .ToList();
            }

            var fontItems = new List<FontListItem>();
            foreach (var f in fonts)
            {
                string src = !string.IsNullOrEmpty(f.Link)
                    ? f.Link
                    : Url.Content($"~/fonts/{f.LocalFileName}");

                var filterIds = _filterRepository
                    .GetFiltersForAsset("Font", f.Id)
                    .Select(x => x.Id)
                    .ToList();

                bool isLocalFile = !string.IsNullOrEmpty(f.LocalFileName);
                string cssName = isLocalFile
                    ? $"{f.FontFamily}_{f.Id}"
                    : f.FontFamily;

                fontItems.Add(new FontListItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    FontFamily = f.FontFamily,
                    LinkOrLocalUrl = src,
                    FilterIds = filterIds,
                    IsFavorited = userFavFontIds.Contains(f.Id),
                    CssFontFamily = cssName
                });
            }

            var vm = new FontIndexViewModel
            {
                AvailableFilters = filterSelectItems,
                Fonts = fontItems,
                InputText = "Пример текста"
            };

            return View(vm);
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpGet]
        public IActionResult AddFont()
        {
            var vm = new CreateFontViewModel
            {
                AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Font")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList()
            };

            return View(vm);
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFont(CreateFontViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Font")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(viewModel);
            }

            var font = new FontData
            {
                Name = viewModel.Name,
                FontFamily = viewModel.FontFamily
            };

            if (!string.IsNullOrEmpty(viewModel.Link))
            {
                font.Link = viewModel.Link;
                font.LocalFileName = null;
            }
            if (viewModel.FontFile != null)
            {
                font.Link = null;
                var fileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
                font.LocalFileName = fileName;
            }


            _fontRepository.AddAsset(font);

            var toBindFilterIds = viewModel.SelectedFilterIds.Distinct().ToList();
            foreach (var fid in toBindFilterIds)
            {
                _filterRepository.AddAssetFilter(new AssetFilter
                {
                    FilterId = fid,
                    AssetType = "Font",
                    AssetId = font.Id
                });
            }

            var newNames = (viewModel.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var name in newNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Font")
                    .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                int newFid;
                if (already != null)
                {
                    newFid = already.Id;
                }
                else
                {
                    var newFilter = new FilterData { Name = name, AssetType = "Font" };
                    _filterRepository.AddAsset(newFilter);
                    newFid = newFilter.Id;
                }

                if (!toBindFilterIds.Contains(newFid))
                {
                    _filterRepository.AddAssetFilter(new AssetFilter
                    {
                        FilterId = newFid,
                        AssetType = "Font",
                        AssetId = font.Id
                    });
                }
            }

            return RedirectToAction(nameof(ShowFonts));
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpGet]
        public IActionResult EditFont(int id)
        {
            var font = _fontRepository.GetAsset(id);
            if (font == null) return NotFound();

            var vm = new EditFontViewModel
            {
                Id = font.Id,
                Name = font.Name,
                FontFamily = font.FontFamily,
                Link = font.Link,
                ExistingFileName = font.LocalFileName
            };

            vm.AvailableFilters = _filterRepository
                .GetFiltersByCategory("Font")
                .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                .ToList();

            vm.SelectedFilterIds = _filterRepository
                .GetFiltersForAsset("Font", id)
                .Select(f => f.Id)
                .ToList();

            return View(vm);
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFont(EditFontViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableFilters = _filterRepository
                    .GetFiltersByCategory("Font")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                return View(viewModel);
            }

            var font = _fontRepository.GetAsset(viewModel.Id);
            if (font == null) return NotFound();

            if (!string.IsNullOrEmpty(viewModel.Link))
            {
                if (!string.IsNullOrEmpty(font.LocalFileName))
                {
                    _fileService.DeleteFile(font.LocalFileName, "fonts");
                    font.LocalFileName = null;
                }
                font.Link = viewModel.Link;
            }

            if (viewModel.FontFile != null)
            {
                if (!string.IsNullOrEmpty(font.Link))
                    font.Link = null;

                if (!string.IsNullOrEmpty(font.LocalFileName))
                    _fileService.DeleteFile(font.LocalFileName, "fonts");

                var newFileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
                font.LocalFileName = newFileName;
            }

            font.Name = viewModel.Name;
            font.FontFamily = viewModel.FontFamily;
            _fontRepository.UpdateAsset(font);


            var currentFilterIds = _filterRepository
                .GetFiltersForAsset("Font", viewModel.Id)
                .Select(f => f.Id)
                .ToList();

            foreach (var oldFid in currentFilterIds)
            {
                if (!viewModel.SelectedFilterIds.Contains(oldFid))
                {
                    _filterRepository.RemoveAssetFilter("Font", viewModel.Id, oldFid);
                }
            }

            var newNames = (viewModel.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var newFilterIds = new List<int>();
            foreach (var name in newNames)
            {
                var already = _filterRepository
                    .GetFiltersByCategory("Font")
                    .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (already != null)
                {
                    newFilterIds.Add(already.Id);
                }
                else
                {
                    var newFilter = new FilterData { Name = name, AssetType = "Font" };
                    _filterRepository.AddAsset(newFilter);
                    newFilterIds.Add(newFilter.Id);
                }
            }

            var finalFilterIds = viewModel.SelectedFilterIds
                .Concat(newFilterIds)
                .Distinct()
                .ToList();

            foreach (var fid in finalFilterIds)
            {
                if (!currentFilterIds.Contains(fid))
                {
                    _filterRepository.AddAssetFilter(new AssetFilter
                    {
                        FilterId = fid,
                        AssetType = "Font",
                        AssetId = font.Id
                    });
                }
            }

            return RedirectToAction(nameof(ShowFonts));
        }


        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var font = _fontRepository.GetAsset(id);
            if (font == null) return NotFound();

            if (!string.IsNullOrEmpty(font.LocalFileName))
                _fileService.DeleteFile(font.LocalFileName, "fonts");

            _fontRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowFonts));
        }
    }
}
