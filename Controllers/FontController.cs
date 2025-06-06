// FrontendHelper/Controllers/FontController.cs
using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class FontController : Controller
    {
        private readonly FontRepository _fontRepository;
        private readonly FilterRepository _filterRepository;
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _authService;
        private readonly IFileService _fileService;

        // Допустимые расширения для локальных файлов
        private readonly string[] _allowedExtensions = new[] { ".ttf", ".otf", ".woff", ".woff2" };

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

        // ========================================
        // Просмотр всех шрифтов
        // ========================================
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
                    .Where(f => f.AssetType == "Font")
                    .Select(f => f.AssetId)
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

                fontItems.Add(new FontListItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    FontFamily = f.FontFamily,
                    LinkOrLocalUrl = src,
                    FilterIds = filterIds,
                    IsFavorited = userFavFontIds.Contains(f.Id)
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


        // ========================================
        // Добавление нового шрифта
        // ========================================
        [HasPermission(Permission.CanManageFonts)]
        [HttpGet]
        public IActionResult AddFont()
        {
            return View(new CreateFontViewModel());
        }

        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFont(CreateFontViewModel viewModel)
        {
            // 1) Если базовая валидация не прошла, возвращаем форму
            if (!ModelState.IsValid)
                return View(viewModel);

            // 2) Если указана локальная загрузка, проверяем расширение
            if (viewModel.FontFile != null)
            {
                var ext = Path.GetExtension(viewModel.FontFile.FileName)?.ToLowerInvariant();
                if (!_allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError(nameof(viewModel.FontFile),
                        "Поддерживаемые форматы: .ttf, .otf, .woff, .woff2");
                    return View(viewModel);
                }
            }

            // 3) Создаём запись в базе
            var font = new FontData
            {
                Name = viewModel.Name,
                FontFamily = viewModel.FontFamily,
                Link = viewModel.Link
            };

            // 4) Сохраняем файл, если он прислан
            if (viewModel.FontFile != null)
            {
                var fileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
                font.LocalFileName = fileName;
            }

            _fontRepository.AddAsset(font);
            return RedirectToAction(nameof(ShowFonts));
        }


        // ========================================
        // Редактирование шрифта
        // ========================================
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

            // Проверяем расширение у нового файла (если он пришёл)
            if (viewModel.FontFile != null)
            {
                var ext = Path.GetExtension(viewModel.FontFile.FileName)?.ToLowerInvariant();
                if (!_allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError(nameof(viewModel.FontFile),
                        "Поддерживаемые форматы: .ttf, .otf, .woff, .woff2");
                    return View(viewModel);
                }
            }

            // Обновляем простые поля
            font.Name = viewModel.Name;
            font.FontFamily = viewModel.FontFamily;
            font.Link = viewModel.Link;

            // Если пришёл новый файл – заменяем старый
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


        // ========================================
        // Удаление шрифта
        // ========================================
        [HasPermission(Permission.CanManageFonts)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var font = _fontRepository.GetAsset(id);
            if (font == null) return NotFound();

            if (!string.IsNullOrEmpty(font.LocalFileName))
            {
                _fileService.DeleteFile(font.LocalFileName, "fonts");
            }

            _fontRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowFonts));
        }
    }
}
