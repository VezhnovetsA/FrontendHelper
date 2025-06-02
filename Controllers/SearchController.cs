// FrontendHelper/Controllers/SearchController.cs
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
        private readonly FilterRepository _filterRepo;

        public SearchController(
            IconRepository iconRepo,
            PictureRepository picRepo,
            AnimatedElementRepository animRepo,
            ButtonRepository buttonRepo,
            TemplateRepository tplRepo,
            FormRepository formRepo,
            FontRepository fontRepo,
            PaletteRepository paletteRepo,
            FilterRepository filterRepo
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
        }

        [HttpGet]
        public IActionResult Index(
            string? searchQuery,
            string? resourceTypeFilter,
            int[]? selectedFilters
        )
        {
            // 1) Подготовка VM и ViewBag
            var vm = new SearchViewModel
            {
                Query = searchQuery ?? "",
                ResourceType = resourceTypeFilter
            };

            if (selectedFilters != null && selectedFilters.Length > 0)
            {
                vm.SelectedFilterIds = selectedFilters.ToList();
            }

            // 2) Загружаем доступные фильтры из БД (если задан тип ресурса)
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

            // Кладём во ViewBag, чтобы Layout мог отрисовать панель фильтров
            ViewBag.AvailableFilters = vm.AvailableFilters;
            ViewBag.SelectedFilterIds = vm.SelectedFilterIds;
            ViewBag.CurrentQuery = vm.Query;
            ViewBag.CurrentResourceType = vm.ResourceType ?? "";

            // 3) Основной поиск: если есть текст OR выбран хотя бы один фильтр
            if (!string.IsNullOrWhiteSpace(vm.Query) || vm.SelectedFilterIds.Any())
            {
                var allResults = new List<SearchResultItem>();

                // Иконки
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Icon")
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

                // Картинки
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Picture")
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

                // Анимированные
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "AnimatedElement")
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

                // Кнопки
                // Кнопки (fix: заполняем и PreviewUrl, и DownloadUrl)
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Button")
                {
                    allResults.AddRange(
                        PerformSearchWithFilters(
                            _buttonRepo,
                            "Button",
                            b => b.Name,
                            _ => (string?)null,
                            _ => string.Empty,   // PreviewUrl пока заполним ниже вручную
                            vm.Query,
                            vm.SelectedFilterIds
                        )
                        .Select(item =>
                        {
                            // Возьмём из репозитория сам объект ButtonData, чтобы получить ButtonCode:
                            var buttonEntity = _buttonRepo.GetAsset(item.Id);
                            // Сформируем URL, который будет открыт в iframe:
                            var url = Url.Content($"~/buttons/{buttonEntity.ButtonCode}");
                            item.PreviewUrl = url;
                            item.DownloadUrl = url;
                            return item;
                        })
                        .ToList()
                    );
                }


                // Шаблоны
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Template")
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
                            // Заполняем DownloadUrl (имя файла TemplateCode из БД)
                            var tpl = _tplRepo.GetAsset(item.Id);
                            item.PreviewUrl = Url.Content($"~/templates/{tpl.TemplateCode}");
                            item.DownloadUrl = Url.Content($"~/templates/{tpl.TemplateCode}");
                            return item;
                        })
                        .ToList()
                    );
                }

                // Формы
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Form")
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
                            // Заполняем DownloadUrl (имя файла FormCode из БД)
                            var form = _formRepo.GetAsset(item.Id);
                            item.PreviewUrl = Url.Content($"~/forms/{form.FormCode}");
                            item.DownloadUrl = Url.Content($"~/forms/{form.FormCode}");
                            return item;
                        })
                        .ToList()
                    );
                }

                // Шрифты
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Font")
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
                            // Заполняем FontFamily (из БД) — чтобы в представлении отобразился пример текста
                            var font = _fontRepo.GetAsset(item.Id);
                            item.FontFamily = font.FontFamily;
                            return item;
                        })
                        .ToList()
                    );
                }

                // Палитры
                if (string.IsNullOrEmpty(resourceTypeFilter) || resourceTypeFilter == "Palette")
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

                // ** 4) После формирования vm.Results, для каждого элемента "Palette" 
                // загружаем цвета из БД и заполняем PaletteColors **
                foreach (var item in vm.Results.Where(r => r.ResourceType == "Palette"))
                {
                    // Берём саму сущность PaletteData с включённым navigation property Colors
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

                    // Также, если надо, можно задать DownloadUrl (например, файл-JSON или какая-то ссылка).
                    // Мы не делаем ссылку для палитры, т.к. копирование цвета выполняется по клику на свотч.
                }
            }

            return View(vm);
        }

        /// <summary>
        /// Общая функция поиска (по тексту + фильтрам). 
        /// Если searchQuery пуст, возвращает сразу все записи данного типа, 
        /// после чего применяется фильтрация по selectedFilterIds.
        /// </summary>
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

            // 1) Если задан текст поиска → накладываем LIKE; иначе оставляем всю коллекцию
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

            // 2) Если выбраны какие-то фильтры → накладываем условие, что AssetFilter показывает все выбранные фильтры
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

            // 3) Проекция в SearchResultItem (дошла очередь ставить PreviewUrl, остальное заполнится позже)
            return query
                .Select(entity => new SearchResultItem
                {
                    ResourceType = resourceTypeName,
                    Id = entity.Id,
                    Name = nameSelector.Compile()(entity),
                    // Topic селектируется только для тех сущностей, у кого есть topicSelector
                    Topic = topicSelector != null ? topicSelector.Compile()(entity) : null,
                    PreviewUrl = previewUrlSelector(entity),
                    // Остальное (DownloadUrl, FontFamily, PaletteColors) будет заполнено уже в Index-процессе
                })
                .ToList();
        }
    }
}
