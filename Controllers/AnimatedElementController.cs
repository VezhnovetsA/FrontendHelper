using FrontendHelper.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class AnimatedElementController : Controller
    {
        private readonly IAllAnimatedElements animatedElementRepository;

        public AnimatedElementController(IAllAnimatedElements animatedElementRepository)
        {
            this.animatedElementRepository = animatedElementRepository;
        }

        public IActionResult AnimatedElementList()
        {
            var animatedElements = animatedElementRepository.AnimatedElements;
            return View(animatedElements);
        }
    }
}
