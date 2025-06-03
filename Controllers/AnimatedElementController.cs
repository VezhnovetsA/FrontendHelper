using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class AnimatedElementController : Controller
    {
        private readonly AnimatedElementRepository _animatedElementRepository;
        private readonly IFileService _fileService;

        public AnimatedElementController(
            AnimatedElementRepository animatedElementRepository,
            IFileService fileService)
        {
            _animatedElementRepository = animatedElementRepository;
            _fileService = fileService;
        }

        // ===========================
        // ПРОСМОТР (CanViewAnimatedElements)
        // ===========================

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowAllAnimatedElements()
        {
            var data = _animatedElementRepository.GetAssets();
            var vm = data.Select(PassDataToViewModel).ToList();
            return View(vm);
        }

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowAllAnimatedElementsOnTheTopic(string topic)
        {
            var data = _animatedElementRepository.GetAllAnimatedElementsByTopic(topic);
            var vm = data.Select(PassDataToViewModel).ToList();
            ViewBag.Topic = topic;
            return View(vm);
        }

        [HasPermission(Permission.CanViewAnimatedElements)]
        public IActionResult ShowGroupsOfAnimatedElementsOnTheTopic(int numberOfIcons = 6)
        {
            var topics = _animatedElementRepository.GetAnimatedElementTopics();
            var vm = topics.Select(t => new AnimatedElementGroupViewModel
            {
                Topic = t,
                AnimatedElements = _animatedElementRepository
                    .GetAnimatedElementGroupByTopic(t, numberOfIcons)
                    .Select(PassDataToViewModel)
                    .ToList()
            });
            return View(vm);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageAnimatedElements)
        // ===========================

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpGet]
        public IActionResult CreateAnimatedElement()
        {
            return View(new CreateAnimatedElementViewModel());
            // CreateAnimatedElementViewModel: { string Name; string Topic; IFormFile ImgFile; }
        }

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnimatedElement(CreateAnimatedElementViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var savedFile = await _fileService.SaveFileAsync(vm.ImgFile, "images/animated-elements");
            var entity = new AnimatedElementData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFile
            };
            _animatedElementRepository.AddAsset(entity);
            return RedirectToAction(nameof(ShowAllAnimatedElements));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageAnimatedElements)
        // ===========================

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpGet]
        public IActionResult EditAnimatedElement(int id)
        {
            var data = _animatedElementRepository.GetAsset(id);
            if (data == null) return NotFound();

            var vm = new EditAnimatedElementViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingImg = data.Img
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnimatedElement(EditAnimatedElementViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var data = _animatedElementRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            data.Name = vm.Name;
            data.Topic = vm.Topic;
            if (vm.ImgFile != null)
            {
                _fileService.DeleteFile(data.Img, "images/animated-elements");
                var newName = await _fileService.SaveFileAsync(vm.ImgFile, "images/animated-elements");
                data.Img = newName;
            }
            _animatedElementRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllAnimatedElements));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageAnimatedElements)
        // ===========================

        [HasPermission(Permission.CanManageAnimatedElements)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAnimatedElement(int id)
        {
            var data = _animatedElementRepository.GetAsset(id);
            if (data == null) return NotFound();

            _fileService.DeleteFile(data.Img, "images/animated-elements");
            _animatedElementRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllAnimatedElements));
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private AnimatedElementViewModel PassDataToViewModel(AnimatedElementData d)
        {
            return new AnimatedElementViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Topic = d.Topic,
                Img = d.Img
            };
        }
    }
}
