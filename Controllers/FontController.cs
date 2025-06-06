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
            // 1) Получаем все записи из репозитория шрифтов
            var fonts = _fontRepository.GetAssets().ToList();

            // 2) Формируем список доступных фильтров (для панели фильтрации)
            var allFilters = _filterRepository.GetFiltersByCategory("Font").ToList();
            var filterSelectItems = allFilters
                .Select(f => new SelectListItem(f.Name, f.Id.ToString()))
                .ToList();

            // 3) Узнаём ID текущего пользователя (если он авторизован), 
            //    чтобы пометить уже добавленные в избранное шрифты
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

            // 4) Собираем список моделей для представления (FontListItem).
            //    В частности, для локальных файлов (LocalFileName != null) 
            //    генерируем CSS-имя вида "FontFamily_<Id>", 
            //    а для внешних ссылок просто "FontFamily".
            var fontItems = new List<FontListItem>();
            foreach (var f in fonts)
            {
                // 4.1) Решаем, откуда брать контент: внешняя ссылка (Google Fonts и т.п.)
                //      или локальный файл (~/fonts/...)
                string src = !string.IsNullOrEmpty(f.Link)
                    ? f.Link
                    : Url.Content($"~/fonts/{f.LocalFileName}");

                // 4.2) Список ID фильтров, привязанных к этому шрифту
                var filterIds = _filterRepository
                    .GetFiltersForAsset("Font", f.Id)
                    .Select(x => x.Id)
                    .ToList();

                // 4.3) Определяем, под каким "CSS-именем" регистрировать font-face:
                //      • если LocalFileName не пустой → локальный файл → присоединяем "_{Id}"
                //      • если LocalFileName пустой, но есть f.Link → внешний CSS → оставляем оригинальное f.FontFamily
                bool isLocalFile = !string.IsNullOrEmpty(f.LocalFileName);
                string cssName = isLocalFile
                    ? $"{f.FontFamily}_{f.Id}"
                    : f.FontFamily;

                fontItems.Add(new FontListItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    FontFamily = f.FontFamily,     // оригинальное имя (отображаем в колонке "CSS-семейство")
                    LinkOrLocalUrl = src,               // или внешний URL, или "/fonts/имя_файла"
                    FilterIds = filterIds,
                    IsFavorited = userFavFontIds.Contains(f.Id),
                    CssFontFamily = cssName            // именно это имя будет использоваться в <font-face> и style="font-family: …"
                });
            }

            // 5) Собираем ViewModel и возвращаем представление
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
            if (!ModelState.IsValid)
                return View(viewModel);

            var font = new FontData
            {
                Name = viewModel.Name,
                FontFamily = viewModel.FontFamily
            };

            // Если пришла ссылка, то явно сбрасываем LocalFileName
            if (!string.IsNullOrEmpty(viewModel.Link))
            {
                font.Link = viewModel.Link;
                font.LocalFileName = null;
            }
            // Если же загрузили файл, то сбрасываем Link и сохраняем только файл
            if (viewModel.FontFile != null)
            {
                font.Link = null;
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

            // 1) Если пришёл новый внешний URL, то сбрасываем локальный файл.
            if (!string.IsNullOrEmpty(viewModel.Link))
            {
                // Если ранее был загружен файл, удаляем его с диска
                if (!string.IsNullOrEmpty(font.LocalFileName))
                {
                    _fileService.DeleteFile(font.LocalFileName, "fonts");
                    font.LocalFileName = null;
                }

                // Обновляем только поле Link
                font.Link = viewModel.Link;
                // (Оставляем font.FontFamily и font.Name, их обновим ниже)
            }

            // 2) Если пользователь загрузил новый файл (FontFile), то сбросим Link и сохраним файл
            if (viewModel.FontFile != null)
            {
                // Если до этого у записи был внешний URL, то сбросим его
                if (!string.IsNullOrEmpty(font.Link))
                {
                    font.Link = null;
                }

                // И если уже лежал локальный файл — удалим сначала его
                if (!string.IsNullOrEmpty(font.LocalFileName))
                {
                    _fileService.DeleteFile(font.LocalFileName, "fonts");
                }
                // Сохраняем новый загруженный файл:
                var newFileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
                font.LocalFileName = newFileName;
            }

            // 3) Обновляем остальные поля (Name, FontFamily).  
            //    Это делаем уже после обработки “Link / LocalFileName”, чтобы не затирать их.
            font.Name = viewModel.Name;
            font.FontFamily = viewModel.FontFamily;

            // 4) Записываем изменения
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
