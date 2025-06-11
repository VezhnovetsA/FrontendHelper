using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Models;
using FrontendHelper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

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
        private readonly FavoriteRepository _favoriteRepo;
        private readonly AuthService _authService;

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
            FavoriteRepository favoriteRepo,
            AuthService authService
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
            _favoriteRepo = favoriteRepo;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index(
            string? searchQuery,
            string? resourceTypeFilter,
            int[]? selectedFilters
        )
        {

            var vm = new SearchViewModel
            {
                Query = searchQuery ?? "",
                ResourceType = resourceTypeFilter
            };

            if (selectedFilters != null && selectedFilters.Length > 0)
            {
                vm.SelectedFilterIds = selectedFilters.ToList();
            }

            if (!string.IsNullOrEmpty(resourceTypeFilter))
            {
                var filtersFromDb = _filterRepo.GetFiltersByCategory(resourceTypeFilter);
                vm.AvailableFilters = filtersFromDb
                    .Select(f => new FilterViewModel { Id = f.Id, Name = f.Name })
                    .ToList();
            }
            else
            {
                vm.AvailableFilters = new List<FilterViewModel>();
            }

            ViewBag.AvailableFilters = vm.AvailableFilters;
            ViewBag.SelectedFilterIds = vm.SelectedFilterIds;
            ViewBag.CurrentQuery = vm.Query;
            ViewBag.CurrentResourceType = vm.ResourceType ?? "";

            if (!string.IsNullOrWhiteSpace(vm.Query) || vm.SelectedFilterIds.Any())
            {
                var allResults = new List<SearchResultItem>();

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Icon" && _authService.HasPermission(Permission.CanViewIcons))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _iconRepo,
                            "Icon",
                            i => i.Name,
                            i => i.Topic,
                            i => Url.Content($"~/images/icons/{i.Img}"),
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Picture" && _authService.HasPermission(Permission.CanViewPictures))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _picRepo,
                            "Picture",
                            p => p.Name,
                            p => p.Topic,
                            p => Url.Content($"~/images/pictures/{p.Img}"),
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "AnimatedElement" && _authService.HasPermission(Permission.CanViewAnimatedElements))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _animRepo,
                            "AnimatedElement",
                            a => a.Name,
                            a => a.Topic,
                            a => Url.Content($"~/images/animated-elements/{a.Img}"),
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Button" && _authService.HasPermission(Permission.CanViewButtons))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _buttonRepo,
                            "Button",
                            b => b.Name,
                            _ => (string?)null,
                            _ => string.Empty, 
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                        .Select(item =>
                        {

                            var buttonEntity = _buttonRepo.GetAsset(item.Id);
                            var url = Url.Content($"~/buttons/{buttonEntity.ButtonCode}");
                            item.PreviewUrl = url;
                            item.DownloadUrl = url;
                            return item;
                        })
                        .ToList()
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Template" && _authService.HasPermission(Permission.CanViewTemplates))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _tplRepo,
                            "Template",
                            t => t.Name,
                            _ => (string?)null,
                            _ => string.Empty,
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                        .Select(item =>
                        {
                            var tpl = _tplRepo.GetAsset(item.Id);
                            item.PreviewUrl = Url.Content($"~/templates/{tpl.TemplateCode}");
                            item.DownloadUrl = Url.Content($"~/templates/{tpl.TemplateCode}");
                            return item;
                        })
                        .ToList()
                    );
                }


                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Form" && _authService.HasPermission(Permission.CanViewForms))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _formRepo,
                            "Form",
                            f => f.Name,
                            _ => (string?)null,
                            _ => string.Empty,
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                        .Select(item =>
                        {
                            var form = _formRepo.GetAsset(item.Id);
                            item.PreviewUrl = Url.Content($"~/forms/{form.FormCode}");
                            item.DownloadUrl = Url.Content($"~/forms/{form.FormCode}");
                            return item;
                        })
                        .ToList()
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Font" && _authService.HasPermission(Permission.CanViewFonts))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _fontRepo,
                            "Font",
                            f => f.Name,
                            _ => (string?)null,
                            _ => string.Empty,
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                        .Select(item =>
                        {
                            var font = _fontRepo.GetAsset(item.Id);
                            item.FontFamily = font.FontFamily;
                            return item;
                        })
                        .ToList()
                    );
                }

                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Palette" && _authService.HasPermission(Permission.CanViewPalettes))
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _paletteRepo,
                            "Palette",
                            p => p.Title,
                            _ => (string?)null,
                            _ => string.Empty,
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                    );
                }

                vm.Results = allResults
                    .OrderBy(r => r.ResourceType)
                    .ThenBy(r => r.Name)
                    .ToList();

                int userId = 0;
                if (_authService.IsAuthenticated())
                {
                    userId = _authService.GetUserId();
                }

                foreach (var item in vm.Results)
                {
                    item.IsFavorited = (userId > 0)
                        ? _favoriteRepo.IsFavorited(userId, item.ResourceType, item.Id)
                        : false;
                }

                foreach (var item in vm.Results.Where(r => r.ResourceType == "Palette"))
                {

                    var paletteEntity = _paletteRepo.GetOnePalette(item.Id);
                    if (paletteEntity != null && paletteEntity.Colors != null)
                    {
                        item.PaletteColors = paletteEntity.Colors
                            .Select(c => new SearchColorViewModel { Hex = c.Hex })
                            .ToList();
                    }
                    else
                    {
                        item.PaletteColors = new List<SearchColorViewModel>();
                    }


                }
            }

            return View(vm);
        }

 
        private List<SearchResultItem> PerformSearchWithFilters<TData>(
            BaseRepository<TData> repository,
            string resourceTypeName,
            Expression<Func<TData, string>> nameSelector,
            Expression<Func<TData, string?>> topicSelector,
            Func<TData, string> previewUrlSelector,
            string searchQuery,
            List<int> selectedFilterIds
        ) where TData : BaseDataModel
        {
            IQueryable<TData> query = repository.Query();


            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string likePattern = $"%{searchQuery}%";
                var nameMember = (MemberExpression)nameSelector.Body;
                string nameProp = nameMember.Member.Name;
                var topicMember = topicSelector?.Body as MemberExpression;
                string topicProp = topicMember?.Member.Name ?? "";

                query = query.Where(entity =>
                    EF.Functions.Like(EF.Property<string>(entity, nameProp), likePattern)
                    || (topicProp != "" && EF.Functions.Like(EF.Property<string>(entity, topicProp)!, likePattern))
                );
            }


            if (selectedFilterIds != null && selectedFilterIds.Count > 0)
            {
                query = query.Where(entity =>
                    _filterRepo.QueryAssetFilters()
                        .Where(af =>
                            af.AssetType == resourceTypeName
                            && af.AssetId == EF.Property<int>(entity, "Id")
                            && selectedFilterIds.Contains(af.FilterId)
                        )
                        .Select(af => af.FilterId)
                        .Distinct()
                        .Count()
                        == selectedFilterIds.Count
                );
            }


            return query
                .Select(entity => new SearchResultItem
                {
                    ResourceType = resourceTypeName,
                    Id = entity.Id,
                    Name = nameSelector.Compile()(entity),
                    Topic = topicSelector != null ? topicSelector.Compile()(entity) : null,
                    PreviewUrl = previewUrlSelector(entity),
                })
                .ToList();
        }
    }
}
