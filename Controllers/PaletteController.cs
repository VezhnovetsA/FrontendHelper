using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class PaletteController : Controller
    {
        private readonly PaletteRepository _paletteRepository;

        public PaletteController(PaletteRepository paletteRepository)
        {
            _paletteRepository = paletteRepository;
        }

        // ===========================
        // ПРОСМОТР (CanViewPalettes)
        // ===========================

        [HasPermission(Permission.CanViewPalettes)]
        public IActionResult ShowAllPalettes()
        {
            var data = _paletteRepository.GetAllPalettes();
            var vms = data.Select(PassDataToViewModel).ToList();
            return View(vms);
        }

        [HasPermission(Permission.CanViewPalettes)]
        public IActionResult ShowOnePalette(int id)
        {
            var data = _paletteRepository.GetOnePalette(id);
            if (data == null) return NotFound();

            var vm = PassDataToViewModel(data);
            return View(vm);
        }

        [HasPermission(Permission.CanViewPalettes)]
        [HttpGet]
        public IActionResult DownloadPalette(int id)
        {
            var data = _paletteRepository.GetOnePalette(id);
            if (data == null) return NotFound();

            var dto = new
            {
                data.Id,
                data.Title,
                Colors = data.Colors.Select(c => new { c.Id, c.Name, c.Hex })
            };
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var fileName = $"palette_{data.Id}.json";
            return File(bytes, "application/json; charset=utf-8", fileName);
        }

        // ===========================
        // СОЗДАНИЕ (CanManagePalettes)
        // ===========================

        [HasPermission(Permission.CanManagePalettes)]
        [HttpGet]
        public IActionResult CreatePalette()
        {
            return View(new CreatePaletteViewModel());
            // CreatePaletteViewModel: { string Title; List<PaletteColorViewModel> Colors; }
        }

        [HasPermission(Permission.CanManagePalettes)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePalette(CreatePaletteViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Предположим, что PaletteData имеет навигацию Colors
            var entity = new PaletteData
            {
                Title = vm.Title,
                Colors = vm.Colors.Select(c => new ColorData
                {
                    Name = c.Name,
                    Hex = c.Hex
                }).ToList()
            };
            _paletteRepository.AddAsset(entity);
            return RedirectToAction(nameof(ShowAllPalettes));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManagePalettes)
        // ===========================

        [HasPermission(Permission.CanManagePalettes)]
        [HttpGet]
        public IActionResult EditPalette(int id)
        {
            var data = _paletteRepository.GetOnePalette(id);
            if (data == null) return NotFound();

            var vm = new EditPaletteViewModel
            {
                Id = data.Id,
                Title = data.Title,
                Colors = data.Colors.Select(c => new PaletteColorViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Hex = c.Hex
                }).ToList()
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManagePalettes)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPalette(EditPaletteViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var data = _paletteRepository.GetOnePalette(vm.Id);
            if (data == null) return NotFound();

            data.Title = vm.Title;

            // Сбросим текущие цвета и создадим новые
            data.Colors.Clear();
            foreach (var c in vm.Colors)
            {
                data.Colors.Add(new ColorData
                {
                    Name = c.Name,
                    Hex = c.Hex
                });
            }

            _paletteRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllPalettes));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManagePalettes)
        // ===========================

        [HasPermission(Permission.CanManagePalettes)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePalette(int id)
        {
            var data = _paletteRepository.GetOnePalette(id);
            if (data == null) return NotFound();

            _paletteRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllPalettes));
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private PaletteViewModel PassDataToViewModel(PaletteData d)
        {
            return new PaletteViewModel
            {
                Id = d.Id,
                Title = d.Title,
                Colors = d.Colors.Select(c => new ColorViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Hex = c.Hex
                }).ToList()
            };
        }
    }
}
