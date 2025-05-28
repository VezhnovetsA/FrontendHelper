using FHDatabase.Repositories;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrontendHelper.Controllers
{
    public class FontController : Controller
    {
        private readonly FontRepository _fontRepository;
        private readonly IFileService _fileService;

        public FontController(FontRepository fontRepository, IFileService fileService)
        {
            _fontRepository = fontRepository;
            _fileService = fileService;
        }

        [IsAdmin]
        [HttpGet]
        public IActionResult AddFont()
        {
            return View(new CreateFontViewModel());
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFont(CreateFontViewModel viewModel, IFormFile? fontFile)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var font = new FontData
            {
                Name = viewModel.Name,
                FontFamily = viewModel.FontFamily,
                Link = viewModel.Link
            };

            if (fontFile != null)
            {
                var fileName = await _fileService.SaveFileAsync(fontFile, "fonts");
                font.LocalFileName = fileName;
            }

            _fontRepository.AddAsset(font);
            return RedirectToAction(nameof(ShowFonts));
        }




        [HttpPost]
        public IActionResult Delete(int id)
        {
            _fontRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowFonts));
        }



        public IActionResult ShowFonts()
        {
            var fonts = _fontRepository.GetAssets();

            var items = fonts.Select(f =>
            {
                var src = !string.IsNullOrEmpty(f.Link)
                    ? f.Link
                    : Url.Content($"~/fonts/{f.LocalFileName}");

                var format = !string.IsNullOrEmpty(f.LocalFileName)
                    ? Path.GetExtension(f.LocalFileName).TrimStart('.') : "ttf";

                return new ExtendedSelectListItem
                {
                    Text = f.Name,
                    Value = f.FontFamily,
                    Group = new SelectListGroup { Name = src },
                    DataAttributes = new Dictionary<string, string> { { "data-format", format } }
                };

            }).ToList();

            var viewModel = new FontViewModel
            {
                AvailableFonts = items
            };

            return View(viewModel);
        }
    }
}
