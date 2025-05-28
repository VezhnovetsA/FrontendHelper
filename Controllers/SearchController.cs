using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public SearchController(
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
            _iconRepo = iconRepo;
            _picRepo = picRepo;
            _animRepo = animRepo;
            _buttonRepo = buttonRepo;
            _tplRepo = tplRepo;
            _formRepo = formRepo;
            _fontRepo = fontRepo;
            _paletteRepo = paletteRepo;
        }

        [HttpGet]
        public IActionResult Index(string? searchQuery, string? resourceTypeFilter)
        {
            ViewBag.CurrentQuery = searchQuery ?? "";
            ViewBag.CurrentResourceType = resourceTypeFilter ?? "";

            var vm = new SearchViewModel
            {
                Query = searchQuery ?? "",
                ResourceType = resourceTypeFilter
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // иконки
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Icon")
                    vm.Results.AddRange(PerformSearch(
                        _iconRepo, "Icon", i => i.Name, i => i.Topic,
                        i => Url.Content($"~/images/icons/{i.Img}"),
                        searchQuery));

                // картнки
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Picture")
                    vm.Results.AddRange(PerformSearch(
                        _picRepo, "Picture", p => p.Name, p => p.Topic,
                        p => Url.Content($"~/images/pictures/{p.Img}"),
                        searchQuery));

                // аним элементы
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "AnimatedElement")
                    vm.Results.AddRange(PerformSearch(
                        _animRepo, "AnimatedElement", a => a.Name, a => a.Topic,
                        a => Url.Content($"~/images/animated-elements/{a.Img}"),
                        searchQuery));

                // кнопки
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Button")
                    vm.Results.AddRange(PerformSearch(
                        _buttonRepo, "Button", b => b.Name, _ => (string?)null,
                        _ => string.Empty,
                        searchQuery));

                // шаблоны
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Template")
                    vm.Results.AddRange(PerformSearch(
                        _tplRepo, "Template", t => t.Name, _ => (string?)null,
                        _ => string.Empty,
                        searchQuery));

                // формы
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Form")
                    vm.Results.AddRange(PerformSearch(
                        _formRepo, "Form", f => f.Name, _ => (string?)null,
                        _ => string.Empty,
                        searchQuery));

                // шрифты
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Font")
                    vm.Results.AddRange(PerformSearch(
                        _fontRepo, "Font", f => f.Name, _ => (string?)null,
                        _ => string.Empty,
                        searchQuery));

                // Палитры (Title вместо Name)
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Palette")
                    vm.Results.AddRange(PerformSearch(
                        _paletteRepo, "Palette", p => p.Title, _ => (string?)null,
                        _ => string.Empty,
                        searchQuery));
            }

            return View(vm);
        }

        private List<SearchResultItem> PerformSearch<TData>(
    BaseRepository<TData> repository,
    string resourceTypeName,
    Expression<Func<TData, string>> nameSelector,
    Expression<Func<TData, string?>> topicSelector,
    Func<TData, string> previewUrlSelector,
    string searchQuery
) where TData : BaseDataModel
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<SearchResultItem>();

            string likePattern = $"%{searchQuery}%";

            var nameMember = (MemberExpression)nameSelector.Body;
            string nameProp = nameMember.Member.Name;

            var topicMember = topicSelector?.Body as MemberExpression;
            string topicProp = topicMember?.Member.Name;

            IQueryable<TData> query = repository.Query();

            query = query.Where(entity =>
                EF.Functions.Like(
                    EF.Property<string>(entity, nameProp),
                    likePattern
                )
                || (topicProp != null
                    && EF.Functions.Like(
                        EF.Property<string>(entity, topicProp)!,
                        likePattern
                    ))
            );

            return query
                .Select(entity => new SearchResultItem
                {
                    ResourceType = resourceTypeName,
                    Id = entity.Id,
                    Name = nameSelector.Compile()(entity),
                    PreviewUrl = previewUrlSelector(entity)
                })
                .ToList();
        }
    }


   
}
