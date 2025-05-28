using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class FavoritesController : Controller
    {
        private FavoriteRepository _favoriteRepository;
        private AuthService _authService;
        private IconRepository _iconRepository;

        public FavoritesController(FavoriteRepository favoriteRepository, AuthService authService, IconRepository iconRepository)
        {
            _favoriteRepository = favoriteRepository;
            _authService = authService;
            _iconRepository = iconRepository;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle([FromBody] FavoriteToggleModel model)
        {

            var userId = _authService.GetUserId();

            var isExists = _favoriteRepository
                .GetFavoriteElementByUser(userId)
                .Any(f => f.AssetType == model.AssetType && f.AssetId == model.AssetId);

            if (isExists)
                _favoriteRepository.RemoveFavoriteElement(userId, model.AssetType, model.AssetId);
            else
                _favoriteRepository.AddFavoriteElement(userId, model.AssetType, model.AssetId);

            return Json(new { favorited = !isExists });
        }



        public IActionResult Index()
        {
            var userId = _authService.GetUserId();
            var favs = _favoriteRepository.GetFavoriteElementByUser(userId);

            var viewModel = new FavoritesViewModel
            {
                Favorites = favs.Select(f =>
                {

                    if (f.AssetType == "Icon")
                    {
                        var icon = _iconRepository.GetAsset(f.AssetId);
                        return new FavoriteItemViewModel
                        {
                            AssetType = "Icon",
                            AssetId = icon.Id,
                            Name = icon.Name,
                            PreviewUrl = Url.Content($"~/images/icons/{icon.Img}")
                        };
                    }

                    return null;
                })
                .Where(x => x != null)
                .ToList()!
            };
            return View(viewModel);
        }

    }
}
