using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;

namespace FrontendHelper.Services
{
    public class TemplateConverter : ITemplateConverter
    {
        private readonly TemplateRepository _templateRepository;

        public TemplateConverter(TemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public IEnumerable<TemplateViewModel> GetAllPreviews()
        {
            return _templateRepository.GetAllTemplates()
                .Select(d => MapToViewModel(d, previewOnly: true));
        }

        public TemplateViewModel? GetFullTemplate(int id)
        {
            var d = _templateRepository.GetTemplateById(id);
            return d == null ? null : MapToViewModel(d, previewOnly: false);
        }




        private TemplateViewModel MapToViewModel(TemplateData data, bool previewOnly)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                                    "wwwroot", "templates", data.TemplateCode);
            var rawHtml = File.Exists(path)
                ? File.ReadAllText(path)
                : "<p class=\"text-danger\">Файл не найден</p>";

            var previewHtml = $@"
                <div style=""max-height:600px; overflow:auto; border:1px solid #ddd; padding:0.5rem;"">
                    <fieldset disabled style=""border:none;margin:0;padding:0;"">
                        {rawHtml}
                    </fieldset>
                </div>";

            return new TemplateViewModel
            {
                Id = data.Id,
                Name = data.Name,
                TemplateCode = data.TemplateCode,
                PreviewHtml = previewHtml,
                FullHtml = previewOnly ? null : rawHtml
            };
        }
    }
}
