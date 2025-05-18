using FHDatabase.Repositories;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class ButtonController : Controller
    {
        private ButtonRepository _buttonRepository;

        public ButtonController(ButtonRepository buttonRepository)
        {
            _buttonRepository = buttonRepository;
        }



        public IActionResult ShowAllButtons()
        {
            var buttonDatas = _buttonRepository.GetAssets();

            var viewModels = buttonDatas
                .Select(PassDataToViewModel)
                .ToList();

            return View(viewModels);
        }



        public IActionResult ShowButton(int id)
        {
            var buttonDatas = _buttonRepository.GetAsset(id);
            if (buttonDatas == null)
                return NotFound();

            var viewModel = PassDataToViewModel(buttonDatas);
            return View(viewModel);
        }


        private ButtonViewModel PassDataToViewModel(ButtonData buttonData)
        {
            return new ButtonViewModel
            {
                Id = buttonData.Id,
                Name = buttonData.Name,
                ButtonCode = buttonData.ButtonCode
            };
        }
    }
}
