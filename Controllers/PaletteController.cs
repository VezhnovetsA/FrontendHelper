using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FrontendHelper.Controllers
{
    public class PaletteController : Controller
    {
        private readonly PaletteRepository _paletteRepo;
        private readonly ColorRepository _colorRepo;
        private readonly FilterRepository _filterRepo;
        private readonly FavoriteRepository _favRepo;
        private readonly AuthService _auth;

        public PaletteController(
            PaletteRepository paletteRepo,
            ColorRepository colorRepo,
            FilterRepository filterRepo,
            FavoriteRepository favRepo,
            AuthService auth
        )
        {
            _paletteRepo = paletteRepo;
            _colorRepo = colorRepo;
            _filterRepo = filterRepo;
            _favRepo = favRepo;
            _auth = auth;
        }

        [HasPermission(Permission.CanViewPalettes)]
        public IActionResult ShowPalettes()
        {
            var userId = _auth.IsAuthenticated() ? _auth.GetUserId() : (int?)null;

            // 1) Available filters
            var filters = _filterRepo.GetFiltersByCategory("Palette")
                .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                .ToList();

            // 2) All palettes
            var palData = _paletteRepo.GetAllPalettes();
            var palVm = palData.Select(p =>
            {
                var isFav = userId.HasValue
                    && _favRepo.GetFavoriteElementByUser(userId.Value)
                        .Any(f => f.AssetType == "Palette" && f.AssetId == p.Id);

                return new PaletteListItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    ColorHexes = p.Colors.Select(c => c.Hex).ToList(),
                    FilterIds = _filterRepo.GetFiltersForAsset("Palette", p.Id).Select(f => f.Id).ToList(),
                    IsFavorited = isFav
                };
            }).ToList();

            return View(new PaletteIndexViewModel
            {
                AvailableFilters = filters,
                Palettes = palVm
            });
        }

        // GET: показать форму создания
[HasPermission(Permission.CanManagePalettes)]
[HttpGet]
public IActionResult CreatePalette()
{
    var vm = new CreatePaletteViewModel
    {
        AvailableFilters = _filterRepo
            .GetFiltersByCategory("Palette")
            .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
            .ToList(),
        AvailableColors = _colorRepo.Query()
            .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
            .ToList()
    };
    return View(vm);
}

        // POST: сохранить новую палитру
        [HasPermission(Permission.CanManagePalettes)]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreatePalette(CreatePaletteViewModel vm)
        {
            // 0) чистим пустые NewColors (как сделали) и ModelState по ним
            if (vm.NewColors != null)
            {
                vm.NewColors = vm.NewColors
                    .Where(c => !string.IsNullOrWhiteSpace(c.Hex))
                    .ToList();
                foreach (var k in ModelState.Keys.Where(k => k.StartsWith("NewColors")).ToList())
                    ModelState.Remove(k);
            }

            // 0.1) чистим ModelState по фильтрам и полю NewFilterNames,
            //     чтобы при return View(vm) чекбоксы и поле заполнялись из vm
            foreach (var k in ModelState.Keys
                         .Where(k =>
                             k == nameof(vm.SelectedFilterIds) ||
                             k == nameof(vm.SelectedColorIds) ||
                             k == nameof(vm.NewFilterNames))
                         .ToList())
            {
                ModelState.Remove(k);
            }
            // ————— 0) Убираем пустые NewColors из модели и чистим ModelState —————
            if (vm.NewColors != null)
            {
                vm.NewColors = vm.NewColors
                    .Where(nc => !string.IsNullOrWhiteSpace(nc.Hex))
                    .ToList();
                foreach (var key in ModelState.Keys
                             .Where(k => k.StartsWith("NewColors"))
                             .ToList())
                {
                    ModelState.Remove(key);
                }
            }

            // ————— 1) Валидация —————
            if (!ModelState.IsValid)
            {
                vm.AvailableFilters = _filterRepo
                    .GetFiltersByCategory("Palette")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                vm.AvailableColors = _colorRepo.Query()
                    .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // ————— 2) Создаём новую палитру —————
            var pal = new PaletteData { Title = vm.Title };
            _paletteRepo.AddAsset(pal);

            // ————— 3) Новые цвета —————
            foreach (var hex in vm.NewColors
                               .Select(nc => nc.Hex)
                               .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var exist = _colorRepo.Query().FirstOrDefault(c => c.Hex == hex);
                ColorData color = exist != null
                    ? _colorRepo.GetAsset(exist.Id)
                    : new ColorData { Name = hex, Hex = hex };

                if (exist == null)
                    _colorRepo.AddAsset(color);

                pal.Colors.Add(color);
            }

            // ————— 4) Существующие цвета —————
            foreach (var cid in vm.SelectedColorIds.Distinct())
            {
                pal.Colors.Add(_colorRepo.GetAsset(cid));
            }

            // ————— 5) Старые + новые фильтры —————
            // (a) уже выбранные
            foreach (var fid in vm.SelectedFilterIds.Distinct())
            {
                _filterRepo.AddAssetFilter(new AssetFilter
                {
                    AssetType = "Palette",
                    AssetId = pal.Id,
                    FilterId = fid
                });
            }
            // (b) новые
            foreach (var name in (vm.NewFilterNames ?? "")
                                 .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var f = _filterRepo
                    .GetFiltersByCategory("Palette")
                    .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?? new FilterData { Name = name, AssetType = "Palette" };

                if (f.Id == 0) _filterRepo.AddAsset(f);

                _filterRepo.AddAssetFilter(new AssetFilter
                {
                    AssetType = "Palette",
                    AssetId = pal.Id,
                    FilterId = f.Id
                });
            }

            // ————— 6) Сохраняем и чистим «сиротские» цвета —————
            _paletteRepo.SaveTracked(pal);
            var orphanColors = _colorRepo.Query()
                .Include(c => c.Palettes)
                .Where(c => !c.Palettes.Any())
                .ToList();
            orphanColors.ForEach(c => _colorRepo.RemoveAsset(c.Id));

            return RedirectToAction(nameof(ShowPalettes));
        }



        // PaletteController.cs

        [HttpGet]
        public IActionResult EditPalette(int id)
        {
            var pal = _paletteRepo.GetOnePalette(id); // это tracked-entity
            if (pal == null) return NotFound();

            var existFilterIds = _filterRepo.GetFiltersForAsset("Palette", id)
                                            .Select(f => f.Id).ToList();

            // готовим ViewModel, включая все доступные фильтры
            var vm = new EditPaletteViewModel
            {
                Id = pal.Id,
                Title = pal.Title,
                SelectedColorIds = pal.Colors.Select(c => c.Id).ToList(),
                NewColors = new List<PaletteColorViewModel>(),
                SelectedFilterIds = existFilterIds,
                AvailableFilters = _filterRepo.GetFiltersByCategory("Palette")
                                              .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                                              .ToList(),
                // подгружаем список всех существующих цветов для чекбоксов
                AvailableColors = _colorRepo.Query()    // уберите AsNoTracking, чтобы были tracked 
                                        .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
                                        .ToList()
            };
            return View(vm);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditPalette(EditPaletteViewModel vm)
        {
            // 0) Убираем пустые NewColors и чистим ModelState по ним
            if (vm.NewColors != null)
            {
                vm.NewColors = vm.NewColors
                    .Where(x => !string.IsNullOrWhiteSpace(x.Hex))
                    .ToList();
                foreach (var key in ModelState.Keys
                             .Where(k => k.StartsWith("NewColors"))
                             .ToList())
                    ModelState.Remove(key);
            }

            // 0.1) Чистим ModelState по списковым полям, чтобы форма «увидела» vm.Selected*
            foreach (var key in ModelState.Keys
                         .Where(k => k == "SelectedColorIds"
                                  || k == "SelectedFilterIds"
                                  || k == "NewFilterNames")
                         .ToList())
            {
                ModelState.Remove(key);
            }

            // 1) Валидация
            if (!ModelState.IsValid)
            {
                // восстанавливаем списки для чекбоксов
                vm.AvailableFilters = _filterRepo.GetFiltersByCategory("Palette")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                vm.AvailableColors = _colorRepo.Query()
                    .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var pal = _paletteRepo.GetOnePalette(vm.Id);
            pal.Title = vm.Title;

            // =========== цвета ===========
            pal.Colors.Clear();
            foreach (var hex in vm.NewColors
                               .Where(c => !string.IsNullOrWhiteSpace(c.Hex))
                               .Select(c => c.Hex)
                               .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var exist = _colorRepo.Query().FirstOrDefault(c => c.Hex == hex);
                var color = exist != null
                    ? _colorRepo.GetAsset(exist.Id)
                    : new ColorData { Name = hex, Hex = hex };
                if (exist == null) _colorRepo.AddAsset(color);
                pal.Colors.Add(color);
            }

            foreach (var cid in vm.SelectedColorIds.Distinct())
                pal.Colors.Add(_colorRepo.GetAsset(cid));

            // =========== фильтры ===========
            var currentFids = _filterRepo.GetFiltersForAsset("Palette", vm.Id).Select(f => f.Id).ToList();
            foreach (var oldFid in currentFids.Except(vm.SelectedFilterIds))
                _filterRepo.RemoveAssetFilter("Palette", vm.Id, oldFid);
            foreach (var newFid in vm.SelectedFilterIds.Except(currentFids))
                _filterRepo.AddAssetFilter(new AssetFilter
                {
                    AssetType = "Palette",
                    AssetId = vm.Id,
                    FilterId = newFid
                });

            var extraNames = (vm.NewFilterNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var name in extraNames)
            {
                var exists = _filterRepo.GetFiltersByCategory("Palette")
                                .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                int fid;
                if (exists != null) fid = exists.Id;
                else
                {
                    var f = new FilterData { Name = name, AssetType = "Palette" };
                    _filterRepo.AddAsset(f);
                    fid = f.Id;
                }

                if (!currentFids.Contains(fid))
                {
                    _filterRepo.AddAssetFilter(new AssetFilter
                    {
                        AssetType = "Palette",
                        AssetId = vm.Id,
                        FilterId = fid
                    });
                }
            }

            // сохраняем все изменения
            _paletteRepo.SaveTracked(pal);

            return RedirectToAction(nameof(ShowPalettes));
        }




        [HasPermission(Permission.CanManagePalettes)]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeletePalette(int id)
        {
            var pal = _paletteRepo.GetOnePalette(id);
            if (pal == null) return NotFound();

            // удаляем связи с фильтрами
            foreach (var f in _filterRepo.GetFiltersForAsset("Palette", id))
                _filterRepo.RemoveAssetFilter("Palette", id, f.Id);

            _paletteRepo.RemoveAsset(id);
            return RedirectToAction(nameof(ShowPalettes));
        }
    }
}
