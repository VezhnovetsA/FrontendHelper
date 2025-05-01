using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateConverter _converter;

        public TemplateController(ITemplateConverter converter)
        {
            _converter = converter;
        }

        // GET /Template
        [HttpGet]
        public IActionResult ShowAllTemplates()
        {
            var viewModels = _converter.GetAllPreviews().ToList();
            return View(viewModels);
        }

        // GET /Template/Details/5
        [HttpGet]
        public IActionResult ShowTemplate(int id)
        {
            var viewModel = _converter.GetFullTemplate(id);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }
    }
}
