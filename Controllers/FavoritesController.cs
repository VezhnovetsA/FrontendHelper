using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly AuthService _authService;
        private readonly FavoriteRepository _favRepo;
        private readonly IconRepository _iconRepo;
        private readonly PictureRepository _picRepo;
        private readonly AnimatedElementRepository _animRepo;
        private readonly ButtonRepository _buttonRepo;
        private readonly TemplateRepository _tplRepo;
        private readonly FormRepository _formRepo;
        private readonly FontRepository _fontRepo;
        private readonly PaletteRepository _paletteRepo;

        public FavoritesController(
            AuthService authService,
            FavoriteRepository favRepo,
            IconRepository iconRepo,
            PictureRepository picRepo,
            AnimatedElementRepository animRepo,
            ButtonRepository buttonRepo,
            TemplateRepository tplRepo,
            FormRepository formRepo,
            FontRepository fontRepo,
            PaletteRepository paletteRepo
        )
        {
            _authService = authService;
            _favRepo = favRepo;
            _iconRepo = iconRepo;
            _picRepo = picRepo;
            _animRepo = animRepo;
            _buttonRepo = buttonRepo;
            _tplRepo = tplRepo;
            _formRepo = formRepo;
            _fontRepo = fontRepo;
            _paletteRepo = paletteRepo;
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle([FromBody] FavoriteToggleModel model)
        {

            if (!_authService.IsAuthenticated())
                return Unauthorized(); 

            var userId = _authService.GetUserId();

            var isExists = _favRepo
                .GetFavoriteElementByUser(userId)
                .Any(f => f.AssetType == model.AssetType && f.AssetId == model.AssetId);

            if (isExists)
                _favRepo.RemoveFavoriteElement(userId, model.AssetType, model.AssetId);
            else
                _favRepo.AddFavoriteElement(userId, model.AssetType, model.AssetId);

            return Json(new { favorited = !isExists });
        }



        public IActionResult Index()
        {
            var vm = new HomeIndexViewModel
            {
                UserName = _authService.GetUserName()
            };
            var userId = _authService.GetUserId();
            var favs = _favRepo.GetFavoriteElementByUser(userId);

            vm.Icons = favs
                .Where(f => f.AssetType == "Icon")
                .Select(f => _iconRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => new SearchResultItem
                {
                    ResourceType = "Icon",
                    Id = x.Id,
                    Name = x.Name,
                    PreviewUrl = Url.Content($"~/images/icons/{x.Img}"),
                    IsFavorited = true
                })
                .ToList();

            vm.Pictures = favs
                .Where(f => f.AssetType == "Picture")
                .Select(f => _picRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => new SearchResultItem
                {
                    ResourceType = "Picture",
                    Id = x.Id,
                    Name = x.Name,
                    PreviewUrl = Url.Content($"~/images/pictures/{x.Img}"),
                    IsFavorited = true
                })
                .ToList();

            vm.AnimatedElements = favs
                .Where(f => f.AssetType == "AnimatedElement")
                .Select(f => _animRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => new SearchResultItem
                {
                    ResourceType = "AnimatedElement",
                    Id = x.Id,
                    Name = x.Name,
                    PreviewUrl = Url.Content($"~/images/animated-elements/{x.Img}"),
                    IsFavorited = true
                })
                .ToList();

            vm.Buttons = favs
                .Where(f => f.AssetType == "Button")
                .Select(f => _buttonRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => {
                    var url = Url.Content($"~/buttons/{x.ButtonCode}");
                    return new SearchResultItem
                    {
                        ResourceType = "Button",
                        Id = x.Id,
                        Name = x.Name,
                        PreviewUrl = url,
                        DownloadUrl = url,
                        IsFavorited = true
                    };
                })
                .ToList();

            vm.Templates = favs
                .Where(f => f.AssetType == "Template")
                .Select(f => _tplRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => {
                    var url = Url.Content($"~/templates/{x.TemplateCode}");
                    return new SearchResultItem
                    {
                        ResourceType = "Template",
                        Id = x.Id,
                        Name = x.Name,
                        PreviewUrl = url,
                        DownloadUrl = url,
                        IsFavorited = true
                    };
                })
                .ToList();

            vm.Forms = favs
                .Where(f => f.AssetType == "Form")
                .Select(f => _formRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => {
                    var url = Url.Content($"~/forms/{x.FormCode}");
                    return new SearchResultItem
                    {
                        ResourceType = "Form",
                        Id = x.Id,
                        Name = x.Name,
                        PreviewUrl = url,
                        DownloadUrl = url,
                        IsFavorited = true
                    };
                })
                .ToList();

            vm.Fonts = favs
                .Where(f => f.AssetType == "Font")
                .Select(f => _fontRepo.Query().FirstOrDefault(x => x.Id == f.AssetId))
                .Where(x => x != null)
                .Select(x => new SearchResultItem
                {
                    ResourceType = "Font",
                    Id = x.Id,
                    Name = x.Name,
                    FontFamily = x.FontFamily,
                    IsFavorited = true
                })
                .ToList();

            vm.Palettes = favs
    .Where(f => f.AssetType == "Palette")
    .Select(f => _paletteRepo
        .Query()
        .Include(p => p.Colors) 
        .FirstOrDefault(x => x.Id == f.AssetId))
    .Where(x => x != null)
    .Select(x => new SearchResultItem
    {
        ResourceType = "Palette",
        Id = x.Id,
        Name = x.Title,
        PaletteColors = x.Colors
                          .Select(c => new SearchColorViewModel { Hex = c.Hex })
                          .ToList(),
        IsFavorited = true
    })
    .ToList();

            return View(vm);
        }


    }
}
