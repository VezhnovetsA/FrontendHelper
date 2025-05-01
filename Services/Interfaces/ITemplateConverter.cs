using FrontendHelper.Models;

namespace FrontendHelper.Services.Interfaces
{
    public interface ITemplateConverter
    {
        IEnumerable<TemplateViewModel> GetAllPreviews();
        TemplateViewModel? GetFullTemplate(int id);
    }
}
