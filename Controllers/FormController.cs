using FHDatabase.Models;
using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class FormController : Controller
    {
        public FormRepository _formRepository;

        public FormController(FormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        public IActionResult ShowAllForms()
        {
            var formDatas = _formRepository.GetAssets();

            var viewModels = formDatas
                .Select(PassDataToViewModel)
                .ToList();

            return View(viewModels);
        }


        private FormViewModel PassDataToViewModel(FormData formData)
        {
            return new FormViewModel
            {
                Id = formData.Id,
                Name = formData.Name,
                FormCode = formData.FormCode
            };
        }

        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var form = _formRepository.GetAsset(id);
            if (form == null)
                return NotFound();

            var content = form.FormCode;
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"form_{form.Id}.txt";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }
    }
}
