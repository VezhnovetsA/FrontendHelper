using FHDatabase.Repositories;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateConverter _converter;
        private TemplateRepository _templateRepository;

        public TemplateController(ITemplateConverter converter, TemplateRepository templateRepository)
        {
            _converter = converter;
            _templateRepository = templateRepository;
        }

        [HttpGet]
        public IActionResult ShowAllTemplates()
        {
            var viewModels = _converter.GetAllPreviews().ToList();
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult ShowTemplate(int id)
        {
            var viewModel = _converter.GetFullTemplate(id);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var tpl = _templateRepository.GetTemplateById(id);
            if (tpl == null)
                return NotFound();

            var content = tpl.TemplateCode;
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"template_{tpl.Id}.txt";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }
    }
}
