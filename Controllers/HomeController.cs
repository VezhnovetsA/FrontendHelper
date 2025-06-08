// FrontendHelper/Controllers/HomeController.cs
using FrontendHelper.Models;
using FHDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using FHDatabase.Models;
using FrontendHelper.Services;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;
        private readonly IconRepository _iconRepo;
        private readonly PictureRepository _picRepo;
        private readonly AnimatedElementRepository _animRepo;
        private readonly ButtonRepository _buttonRepo;
        private readonly TemplateRepository _tplRepo;
        private readonly FormRepository _formRepo;
        private readonly FontRepository _fontRepo;
        private readonly PaletteRepository _paletteRepo;
        private readonly FavoriteRepository _favRepo;

        public HomeController(
            AuthService authService,
            IconRepository iconRepo,
            PictureRepository picRepo,
            AnimatedElementRepository animRepo,
            ButtonRepository buttonRepo,
            TemplateRepository tplRepo,
            FormRepository formRepo,
            FontRepository fontRepo,
            PaletteRepository paletteRepo,
            FavoriteRepository favRepo
        )
        {
            _authService = authService;
            _iconRepo = iconRepo;
            _picRepo = picRepo;
            _animRepo = animRepo;
            _buttonRepo = buttonRepo;
            _tplRepo = tplRepo;
            _formRepo = formRepo;
            _fontRepo = fontRepo;
            _paletteRepo = paletteRepo;
            _favRepo = favRepo;
        }

        public IActionResult Index()
        {
            var vm = new HomeIndexViewModel
            {
                UserName = _authService.GetUserName()
            };

            int? userId = _authService.IsAuthenticated()
                ? _authService.GetUserId()
                : (int?)null;

            // Вспомогательный лямбда, чтобы не дублировать код
            static List<SearchResultItem> Load<T>(
     BaseRepository<T> repo,
     string resourceType,
     Func<T, string> nameSelector,
     Func<T, string?> topicSelector,
     Func<T, string> previewUrl,
     Func<T, string?> downloadUrl,
     FavoriteRepository favRepo,
     int? userId,
     int limit
 ) where T : BaseDataModel
            {
                // Забираем первых limit штук из БД
                var entities = repo
                    .Query()
                    .AsNoTracking()
                    .Take(limit)
                    .ToList();

                // Проецируем в SearchResultItem, уже используя лямбды для Name/Topic
                return entities.Select(e => new SearchResultItem
                {
                    ResourceType = resourceType,
                    Id = e.Id,
                    Name = nameSelector(e),          // <-- напрямую
                    Topic = topicSelector(e),         // <-- напрямую
                    PreviewUrl = previewUrl(e),
                    DownloadUrl = downloadUrl(e),
                    IsFavorited = userId.HasValue
                                    && favRepo.IsFavorited(userId.Value, resourceType, e.Id)
                }).ToList();
            }

            // Иконки
            vm.Icons = Load(
                _iconRepo, "Icon",
                i => i.Name,
                i => i.Topic,
                i => Url.Content($"~/images/icons/{i.Img}"),
                _ => null,
                _favRepo,
                userId,
                8
            );

            // Картинки
            vm.Pictures = Load(
                _picRepo, "Picture",
                p => p.Name,
                p => p.Topic,
                p => Url.Content($"~/images/pictures/{p.Img}"),
                _ => null,
                _favRepo,
                userId,
                8
            );

            // Анимации
            vm.AnimatedElements = Load(
                _animRepo, "AnimatedElement",
                a => a.Name,
                a => a.Topic,
                a => Url.Content($"~/images/animated-elements/{a.Img}"),
                _ => null,
                _favRepo,
                userId,
                8
            );

            // Кнопки
            vm.Buttons = Load(
                _buttonRepo, "Button",
                b => b.Name,
                _ => null,
                b => {
                    var bd = _buttonRepo.GetAsset(b.Id);
                    return Url.Content($"~/buttons/{bd.ButtonCode}");
                },
                b => {
                    var bd = _buttonRepo.GetAsset(b.Id);
                    return Url.Content($"~/buttons/{bd.ButtonCode}");
                },
                _favRepo,
                userId,
                8
            );

            // Шаблоны
            vm.Templates = Load(
                _tplRepo, "Template",
                t => t.Name,
                _ => null,
                t => {
                    var td = _tplRepo.GetAsset(t.Id);
                    return Url.Content($"~/templates/{td.TemplateCode}");
                },
                t => {
                    var td = _tplRepo.GetAsset(t.Id);
                    return Url.Content($"~/templates/{td.TemplateCode}");
                },
                _favRepo,
                userId,
                8
            );

            // Формы
            vm.Forms = Load(
                _formRepo, "Form",
                f => f.Name,
                _ => null,
                f => {
                    var fd = _formRepo.GetAsset(f.Id);
                    return Url.Content($"~/forms/{fd.FormCode}");
                },
                f => {
                    var fd = _formRepo.GetAsset(f.Id);
                    return Url.Content($"~/forms/{fd.FormCode}");
                },
                _favRepo,
                userId,
                8
            );

            // Шрифты
            vm.Fonts = Load(
                _fontRepo, "Font",
                f => f.Name,
                _ => null,
                _ => string.Empty,
                _ => null,
                _favRepo,
                userId,
                8
            );

            foreach (var f in vm.Fonts)
            {
                var font = _fontRepo.GetAsset(f.Id);
                f.FontFamily = font?.FontFamily;
            }

            vm.Palettes = Load(
    _paletteRepo,        // репозиторий
    "Palette",           // resourceType
    p => p.Title,        // nameSelector — например, Title
    _ => (string?)null,  // topicSelector — у палитр нет темы
    _ => string.Empty,   // previewUrlSelector — не нужен
    _ => null,           // downloadUrlSelector — не нужен
    _favRepo,            // ваш FavoriteRepository
    userId,              // текущий пользователь
    8                    // лимит
);

            // теперь «дотянем» цвета из БД
            foreach (var p in vm.Palettes)
            {
                var paletteEntity = _paletteRepo.GetOnePalette(p.Id);
                p.PaletteColors = paletteEntity?.Colors
                    .Select(c => new SearchColorViewModel { Hex = c.Hex })
                    .ToList()
                    ?? new List<SearchColorViewModel>();
            }


            return View(vm);
        }
    }
}