using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;

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

        // Показать все шрифты (View)

        [HasPermission(Permission.CanViewFonts)]
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

        // Форма создания нового шрифта

        [HasPermission(Permission.CanManageFonts)]
        [HttpGet]
        public IActionResult AddFont()
        {
            return View(new CreateFontViewModel());
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // Форма редактирования существующего шрифта
        [HasPermission(Permission.CanManageFonts)]
        [HttpGet]
        public IActionResult EditFont(int id)
        {
            var font = _fontRepository.GetAsset(id);
            if (font == null)
                return NotFound();

            var vm = new EditFontViewModel
            {
                Id = font.Id,
                Name = font.Name,
                FontFamily = font.FontFamily,
                Link = font.Link,
                ExistingFileName = font.LocalFileName
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFont(EditFontViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var font = _fontRepository.GetAsset(viewModel.Id);
            if (font == null)
                return NotFound();

            // Обновляем свойства
            font.Name = viewModel.Name;
            font.FontFamily = viewModel.FontFamily;
            font.Link = viewModel.Link;

            // Если загрузили новый файл, заменяем старый:
            if (viewModel.FontFile != null)
            {
                if (!string.IsNullOrEmpty(font.LocalFileName))
                {
                    _fileService.DeleteFile(font.LocalFileName, "fonts");
                }
                var newFileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
                font.LocalFileName = newFileName;
            }

            _fontRepository.UpdateAsset(font);
            return RedirectToAction(nameof(ShowFonts));
        }

        // Удаление шрифта
        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var font = _fontRepository.GetAsset(id);
            if (font == null)
                return NotFound();

            // Удаляем файл с диска, если он есть
            if (!string.IsNullOrEmpty(font.LocalFileName))
            {
                _fileService.DeleteFile(font.LocalFileName, "fonts");
            }

            _fontRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowFonts));
        }
    }
}
