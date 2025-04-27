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
    }
}
