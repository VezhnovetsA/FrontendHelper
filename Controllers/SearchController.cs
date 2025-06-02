using FHDatabase;
using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FrontendHelper.Controllers
{
    public class SearchController : Controller
    {
        private readonly IconRepository _iconRepo;
        private readonly PictureRepository _picRepo;
        private readonly AnimatedElementRepository _animRepo;
        private readonly ButtonRepository _buttonRepo;
        private readonly TemplateRepository _tplRepo;
        private readonly FormRepository _formRepo;
        private readonly FontRepository _fontRepo;
        private readonly PaletteRepository _paletteRepo;
        private readonly FilterRepository _filterRepo;
        private readonly FhDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public SearchController(
            IconRepository iconRepo,
            PictureRepository picRepo,
            AnimatedElementRepository animRepo,
            ButtonRepository buttonRepo,
            TemplateRepository tplRepo,
            FormRepository formRepo,
            FontRepository fontRepo,
            PaletteRepository paletteRepo,
            FilterRepository filterRepo,
            FhDbContext dbContext,
            IWebHostEnvironment env
        )
        {
            _iconRepo = iconRepo;
            _picRepo = picRepo;
            _animRepo = animRepo;
            _buttonRepo = buttonRepo;
            _tplRepo = tplRepo;
            _formRepo = formRepo;
            _fontRepo = fontRepo;
            _paletteRepo = paletteRepo;
            _filterRepo = filterRepo;
            _dbContext = dbContext;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index(string? searchQuery, string? resourceTypeFilter, int[]? selectedFilterIds)
        {
            ViewBag.CurrentQuery = searchQuery ?? "";
            ViewBag.CurrentResourceType = resourceTypeFilter ?? "";
            ViewBag.CurrentFilterIds = selectedFilterIds ?? new int[0];

            var vm = new SearchViewModel
            {
                Query = searchQuery ?? "",
                ResourceType = resourceTypeFilter,
                SelectedFilterIds = (selectedFilterIds ?? Array.Empty<int>()).ToList()
            };

            if (!string.IsNullOrEmpty(resourceTypeFilter))
            {
                var filters = _filterRepo.GetFiltersByCategory(resourceTypeFilter);
                vm.AvailableFilters = filters
                    .Select(f => new FilterViewModel { Id = f.Id, Name = f.Name })
                    .ToList();
            }

            if (string.IsNullOrWhiteSpace(searchQuery))
                return View(vm);

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Icon")
                vm.Results.AddRange(SearchIcons(searchQuery, resourceTypeFilter == "Icon" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Picture")
                vm.Results.AddRange(SearchPictures(searchQuery, resourceTypeFilter == "Picture" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "AnimatedElement")
                vm.Results.AddRange(SearchAnimatedElements(searchQuery, resourceTypeFilter == "AnimatedElement" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Button")
                vm.Results.AddRange(SearchButtons(searchQuery, resourceTypeFilter == "Button" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Template")
                vm.Results.AddRange(SearchTemplates(searchQuery, resourceTypeFilter == "Template" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Form")
                vm.Results.AddRange(SearchForms(searchQuery, resourceTypeFilter == "Form" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Font")
                vm.Results.AddRange(SearchFonts(searchQuery, resourceTypeFilter == "Font" ? vm.SelectedFilterIds : null));

            if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Palette")
                vm.Results.AddRange(SearchPalettes(searchQuery, resourceTypeFilter == "Palette" ? vm.SelectedFilterIds : null));

            return View(vm);
        }

        #region Поиск по каждому типу ресурсов

        private List<SearchResultItem> SearchIcons(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(IconData.Name);
            var topicProp = nameof(IconData.Topic);

            var query = _iconRepo.Query()
                .Where(i =>
                    EF.Functions.Like(EF.Property<string>(i, nameProp), likePattern)
                    || EF.Functions.Like(EF.Property<string>(i, topicProp), likePattern)
                );

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Icon" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(i => assetIds.Contains(i.Id));
            }

            return query
                .AsNoTracking()
                .Select(i => new SearchResultItem
                {
                    ResourceType = "Icon",
                    Id = i.Id,
                    Name = i.Name,
                    Topic = i.Topic,
                    PreviewUrl = Url.Content($"~/images/icons/{i.Img}"),
                    CodeContent = null,
                    DownloadUrl = Url.Content($"~/images/icons/{i.Img}")
                })
                .ToList();
        }

        private List<SearchResultItem> SearchPictures(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(PictureData.Name);
            var topicProp = nameof(PictureData.Topic);

            var query = _picRepo.Query()
                .Where(p =>
                    EF.Functions.Like(EF.Property<string>(p, nameProp), likePattern)
                    || EF.Functions.Like(EF.Property<string>(p, topicProp), likePattern)
                );

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Picture" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(p => assetIds.Contains(p.Id));
            }

            return query
                .AsNoTracking()
                .Select(p => new SearchResultItem
                {
                    ResourceType = "Picture",
                    Id = p.Id,
                    Name = p.Name,
                    Topic = p.Topic,
                    PreviewUrl = Url.Content($"~/images/pictures/{p.Img}"),
                    CodeContent = null,
                    DownloadUrl = Url.Content($"~/images/pictures/{p.Img}")
                })
                .ToList();
        }

        private List<SearchResultItem> SearchAnimatedElements(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(AnimatedElementData.Name);
            var topicProp = nameof(AnimatedElementData.Topic);

            var query = _animRepo.Query()
                .Where(a =>
                    EF.Functions.Like(EF.Property<string>(a, nameProp), likePattern)
                    || EF.Functions.Like(EF.Property<string>(a, topicProp), likePattern)
                );

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "AnimatedElement" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(a => assetIds.Contains(a.Id));
            }

            return query
                .AsNoTracking()
                .Select(a => new SearchResultItem
                {
                    ResourceType = "AnimatedElement",
                    Id = a.Id,
                    Name = a.Name,
                    Topic = a.Topic,
                    PreviewUrl = Url.Content($"~/images/animated-elements/{a.Img}"),
                    CodeContent = null,
                    DownloadUrl = Url.Content($"~/images/animated-elements/{a.Img}")
                })
                .ToList();
        }

        private List<SearchResultItem> SearchButtons(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(ButtonData.Name);

            var query = _buttonRepo.Query()
                .Where(b => EF.Functions.Like(EF.Property<string>(b, nameProp), likePattern));

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Button" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(b => assetIds.Contains(b.Id));
            }

            return query
                .AsNoTracking()
                .Select(b => new SearchResultItem
                {
                    ResourceType = "Button",
                    Id = b.Id,
                    Name = b.Name,
                    Topic = null,
                    // вместо чтения файла разворачиваем URL для iframe
                    PreviewUrl = Url.Content($"~/buttons/{b.ButtonCode}"),
                    CodeContent = null,
                    DownloadUrl = Url.Action("DownloadCode", "Button", new { id = b.Id })
                })
                .ToList();
        }

        private List<SearchResultItem> SearchTemplates(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(TemplateData.Name);

            var query = _tplRepo.Query()
                .Where(t => EF.Functions.Like(EF.Property<string>(t, nameProp), likePattern));

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Template" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(t => assetIds.Contains(t.Id));
            }

            return query
                .AsNoTracking()
                .Select(t => new SearchResultItem
                {
                    ResourceType = "Template",
                    Id = t.Id,
                    Name = t.Name,
                    Topic = null,
                    PreviewUrl = Url.Content($"~/templates/{t.TemplateCode}"),
                    CodeContent = null,
                    DownloadUrl = Url.Content($"~/templates/{t.TemplateCode}")
                })
                .ToList();
        }

        private List<SearchResultItem> SearchForms(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(FormData.Name);

            var query = _formRepo.Query()
                .Where(f => EF.Functions.Like(EF.Property<string>(f, nameProp), likePattern));

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Form" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(f => assetIds.Contains(f.Id));
            }

            return query
                .AsNoTracking()
                .Select(f => new SearchResultItem
                {
                    ResourceType = "Form",
                    Id = f.Id,
                    Name = f.Name,
                    Topic = null,
                    PreviewUrl = Url.Content($"~/forms/{f.FormCode}"),
                    CodeContent = null,
                    DownloadUrl = Url.Content($"~/forms/{f.FormCode}")
                })
                .ToList();
        }

        private List<SearchResultItem> SearchFonts(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var nameProp = nameof(FontData.Name);

            var query = _fontRepo.Query()
                .Where(f => EF.Functions.Like(EF.Property<string>(f, nameProp), likePattern));

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Font" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(f => assetIds.Contains(f.Id));
            }

            return query
                .AsNoTracking()
                .Select(f => new SearchResultItem
                {
                    ResourceType = "Font",
                    Id = f.Id,
                    Name = f.Name,
                    Topic = null,
                    PreviewUrl = f.Link,
                    CodeContent = null,
                    DownloadUrl = f.Link
                })
                .ToList();
        }

        private List<SearchResultItem> SearchPalettes(string searchQuery, List<int>? filterIds)
        {
            var likePattern = $"%{searchQuery}%";
            var titleProp = nameof(PaletteData.Title);

            var query = _paletteRepo.Query()
                .Include(p => p.Colors)
                .Where(p => EF.Functions.Like(EF.Property<string>(p, titleProp), likePattern));

            if (filterIds?.Any() == true)
            {
                var assetIds = _dbContext.AssetFilters
                    .Where(af => af.AssetType == "Palette" && filterIds.Contains(af.FilterId))
                    .Select(af => af.AssetId)
                    .ToList();

                query = query.Where(p => assetIds.Contains(p.Id));
            }

            return query
                .AsNoTracking()
                .Select(p => new SearchResultItem
                {
                    ResourceType = "Palette",
                    Id = p.Id,
                    Name = p.Title,
                    Topic = null,
                    PreviewUrl = null,
                    CodeContent = null,
                    DownloadUrl = Url.Action("DownloadPalette", "Palette", new { id = p.Id })
                })
                .ToList();
        }

        #endregion
    }
}
