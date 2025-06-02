using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FrontendHelper.Controllers
{
    public class PaletteController : Controller
    {

        private PaletteRepository _paletteRepository;

        public PaletteController(PaletteRepository paletteRepository)
        {
            _paletteRepository = paletteRepository;
        }



        public IActionResult ShowAllPalettes()
        {
            var palettes = _paletteRepository.GetAllPalettes();
            var viewModels = palettes
                .Select(PassDataToViewModel)
                .ToList();

            return View(viewModels);
        }


        public IActionResult ShowOnePalette(int id)
        {
            var palette = _paletteRepository.GetOnePalette(id);
            if (palette == null) return NotFound();

            var viewModel = PassDataToViewModel(palette);
            return View(viewModel);
        }



        // маппер
        private PaletteViewModel PassDataToViewModel(PaletteData data)
        {
            return new PaletteViewModel
            {
                Id = data.Id,
                Title = data.Title,
                Colors = data.Colors
                    .Select(c => new ColorViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Hex = c.Hex
                    })
                    .ToList()
            };
        }

        [HttpGet]
        public IActionResult DownloadPalette(int id)
        {
            var palette = _paletteRepository.GetOnePalette(id);
            if (palette == null)
                return NotFound();

            // Сериализуем объект палитры (например, только список цветов в JSON)
            var dto = new
            {
                palette.Id,
                palette.Title,
                Colors = palette.Colors.Select(c => new { c.Id, c.Name, c.Hex })
            };
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var fileName = $"palette_{palette.Id}.json";
            return File(bytes, "application/json; charset=utf-8", fileName);
        }

    }
}
