using FHDatabase.Repositories;
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


        [HttpGet]
        public IActionResult AddFont() => View(new CreateFontViewModel());




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddFont(CreateFontViewModel viewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(viewModel);

        //    var font = new FontData
        //    {
        //        Name = viewModel.Name,
        //        FontFamily = viewModel.FontFamily,
        //        Link = viewModel.Link
        //    };

        //    if (viewModel.FontFile is not null)
        //    {
        //        font.LocalFileName = await _fileService.SaveFileAsync(viewModel.FontFile, "fonts");
        //    }

        //    _fontRepository.AddAsset(font);
        //    return RedirectToAction(nameof(ShowFonts));
        //}



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFont(CreateFontViewModel vm, IFormFile? FontFile)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var entity = new FontData
            {
                Name = vm.Name,
                FontFamily = vm.FontFamily,
                Link = vm.Link
            };

            if (FontFile != null)
            {
                // сохраним локальный файл в ~/wwwroot/fonts
                var fileName = await _fileService.SaveFileAsync(FontFile, "fonts");
                entity.LocalFileName = fileName;
            }

            _fontRepository.AddAsset(entity);
            return RedirectToAction(nameof(ShowFonts));
        }





        [HttpPost]
        public IActionResult Delete(int id)
        {
            _fontRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowFonts));
        }



        //public IActionResult ShowFonts()
        //{
        //    // Получаем все шрифты из репозитория
        //    var fonts = _fontRepository.GetAssets();

        //    // Мапим их в SelectListItem для выпадающего списка
        //    var items = fonts
        //        .Select(f => new SelectListItem(f.Name, f.FontFamily))
        //        .ToList();

        //    // Стартовый текст и пустой выбор шрифта
        //    var viewModel = new FontViewModel
        //    {
        //        InputText = "Пример текста",
        //        SelectedFont = "",
        //        AvailableFonts = items
        //    };

        //    return View(viewModel);
        //}
        public IActionResult ShowFonts()
        {
            var fonts = _fontRepository.GetAssets();

            var items = fonts.Select(f => {
                // определяем путь и формат один раз
                var src = !string.IsNullOrEmpty(f.Link)
                    ? f.Link
                    : Url.Content($"~/fonts/{f.LocalFileName}");

                var format = !string.IsNullOrEmpty(f.LocalFileName)
                    ? Path.GetExtension(f.LocalFileName).TrimStart('.')
                    : "woff2";  // или другой дефолт для внешних

                return new ExtendedSelectListItem
                {
                    Text = f.Name,
                    Value = f.FontFamily,
                    // Используем Group.Name для хранения src; можно создать отдельное поле
                    Group = new SelectListGroup { Name = src },
                    // Добавляем DataAttributes уже готовые
                    DataAttributes = new Dictionary<string, string> {
                { "data-format", format }
            }
                };
            }).ToList();

            var vm = new FontViewModel
            {
                AvailableFonts = items
            };
            return View(vm);
        }
    }
}
