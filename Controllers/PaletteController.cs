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
            if (!ModelState.IsValid)
            {
                // восстановим списки в модели
                vm.AvailableFilters = _filterRepo
                    .GetFiltersByCategory("Palette")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                vm.AvailableColors = _colorRepo.Query()
                    .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            // 1) Создаём и сохраняем новую палитру (теперь она tracked)
            var pal = new PaletteData { Title = vm.Title };
            _paletteRepo.AddAsset(pal);

            // 2) Обрабатываем новые цвета
            foreach (var nc in vm.NewColors)
            {
                // сперва ищем в базе (AsNoTracking), но для добавления в навигацию будем брать через GetAsset
                var exist = _colorRepo.Query().FirstOrDefault(c => c.Hex == nc.Hex);
                ColorData color;
                if (exist != null)
                {
                    // берём tracked-экземпляр
                    color = _colorRepo.GetAsset(exist.Id);
                }
                else
                {
                    color = new ColorData { Name = nc.Hex, Hex = nc.Hex };
                    _colorRepo.AddAsset(color);
                }
                pal.Colors.Add(color);
            }

            // 3) Обрабатываем выбор уже существующих
            foreach (var cid in vm.SelectedColorIds.Distinct())
            {
                var c = _colorRepo.GetAsset(cid);  // tracked
                pal.Colors.Add(c);
            }

            // 4) Привязываем фильтры
            foreach (var fid in vm.SelectedFilterIds.Distinct())
            {
                _filterRepo.AddAssetFilter(new AssetFilter
                {
                    AssetType = "Palette",
                    AssetId = pal.Id,
                    FilterId = fid
                });
            }

            // 5) Сохраняем изменения в самой палитре
            _paletteRepo.SaveTracked(pal);

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
            if (!ModelState.IsValid)
            {
                // восстанавливаем AvailableFilters и AvailableColors
                vm.AvailableFilters = _filterRepo.GetFiltersByCategory("Palette")
                    .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                    .ToList();
                vm.AvailableColors = _colorRepo.Query()
                    .Select(c => new SelectListItem(c.Hex, c.Id.ToString()))
                    .ToList();
                return View(vm);
            }

            var pal = _paletteRepo.GetOnePalette(vm.Id); // tracked
            if (pal == null) return NotFound();

            pal.Title = vm.Title;

            // сбрасываем цвета
            pal.Colors.Clear();

            // 1) новые
            foreach (var nc in vm.NewColors)
            {
                var color = _colorRepo.Query().FirstOrDefault(c => c.Hex == nc.Hex)
                            ?? new ColorData { Name = nc.Hex, Hex = nc.Hex };
                if (color.Id == 0) _colorRepo.AddAsset(color);
                pal.Colors.Add(color);
            }

            // 2) уже существующие — ЗАГРУЖАЕМ ЧЕРЕЗ GetAsset, чтобы они были tracked
            foreach (var cid in vm.SelectedColorIds)
            {
                var color = _colorRepo.GetAsset(cid);
                pal.Colors.Add(color);
            }

            // обновляем фильтры
            var currentFids = _filterRepo.GetFiltersForAsset("Palette", vm.Id).Select(f => f.Id).ToList();
            foreach (var old in currentFids.Except(vm.SelectedFilterIds))
                _filterRepo.RemoveAssetFilter("Palette", vm.Id, old);
            foreach (var fid in vm.SelectedFilterIds.Except(currentFids))
                _filterRepo.AddAssetFilter(new AssetFilter
                {
                    AssetType = "Palette",
                    AssetId = vm.Id,
                    FilterId = fid
                });

            // НЕ вызываем UpdateAsset, просто сохраняем уже-трекед палитру:
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
