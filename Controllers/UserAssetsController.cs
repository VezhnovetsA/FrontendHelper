// FrontendHelper/Controllers/UserAssetsController.cs
using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class UserAssetsController : Controller
{
    private readonly UserAssetRepository _assets;
    private readonly AuthService _auth;
    private readonly IFileService _files;

    public UserAssetsController(
        UserAssetRepository assets,
        AuthService auth,
        IFileService files)
    {
        _assets = assets;
        _auth = auth;
        _files = files;
    }

    public IActionResult Index()
    {
        var userId = _auth.GetUserId();
        var data = _assets.GetByUser(userId)
            .Select(a => new SearchResultItem
            {
                Id = a.Id,
                ResourceType = a.ResourceType,
                Name = a.Name,
                Topic = a.Topic,
                PreviewUrl = Url.Content($"~/uploads/{a.FilePath}"),
                DownloadUrl = Url.Content($"~/uploads/{a.FilePath}"),
                IsFavorited = false
            });
        return View(new UserAssetListViewModel { Items = data });
    }

    public IActionResult Create()
        => View(new CreateUserAssetViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserAssetViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        // сохраняем файл в wwwroot/uploads
        var filename = await _files.SaveFileAsync(m.File, "uploads");

        _assets.AddAsset(new FHDatabase.Models.UserAssetData
        {
            UserId = _auth.GetUserId(),
            Name = m.Name,
            Topic = m.Topic,
            ResourceType = m.ResourceType,
            FilePath = filename
        });
        return RedirectToAction(nameof(Index));
    }
}
