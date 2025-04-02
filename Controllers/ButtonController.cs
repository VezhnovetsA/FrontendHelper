using FrontendHelper.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Controllers
{
    public class ButtonController : Controller
    {
        private readonly IAllButtons buttonRepository;

        public ButtonController(IAllButtons buttonRepository)
        {
            this.buttonRepository = buttonRepository;
        }

        public IActionResult ButtonList()
        {
            var buttons = buttonRepository.Buttons;
                

            return View(buttons);

        }
    }
}
