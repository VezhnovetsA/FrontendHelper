using FrontendHelper.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class FontController : Controller
    {
        private readonly IAllFonts fontRepository;

        public FontController(IAllFonts fontRepository)
        {
            this.fontRepository = fontRepository;
        }

        public IActionResult FontList()
        {
            var fonts = fontRepository.Fonts;
            return View(fonts);
        }
    }
}
