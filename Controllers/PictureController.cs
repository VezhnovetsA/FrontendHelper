using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class PictureController : Controller
    {
        private readonly PictureRepository _pictureRepository;
        private readonly IFileService _fileService;

        public PictureController(PictureRepository pictureRepository, IFileService fileService)
        {
            _pictureRepository = pictureRepository;
            _fileService = fileService;
        }

        // ===========================
        // ПРОСМОТР (CanViewPictures)
        // ===========================

        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowPreviewPictureOnTheTopic(string topic)
        {
            var pictureDatas = _pictureRepository.GetOnePicturePerTopic();
            var viewModels = pictureDatas
                .Where(x => x != null)
                .Select(PassDataToViewModel)
                .ToList();
            return View(viewModels);
        }

        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowAllPicturesOnTheTopic(string topic)
        {
            var data = _pictureRepository.GetAllPicturesByTopic(topic);
            var viewModels = data.Select(PassDataToViewModel).ToList();
            ViewBag.Topic = topic;
            return View(viewModels);
        }

        [HasPermission(Permission.CanViewPictures)]
        public IActionResult ShowAllPictures()
        {
            var data = _pictureRepository.GetAssets(); // если есть метод GetAssets()
            var vms = data.Select(PassDataToViewModel).ToList();
            return View(vms);
        }

        // ===========================
        // СОЗДАНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpGet]
        public IActionResult CreatePicture()
        {
            return View(new CreatePictureViewModel());
            // CreatePictureViewModel: { string Name; string Topic; IFormFile ImgFile; }
        }

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePicture(CreatePictureViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var savedFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
            var pictureData = new PictureData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                Img = savedFileName
            };
            _pictureRepository.AddAsset(pictureData);
            return RedirectToAction(nameof(ShowAllPictures));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpGet]
        public IActionResult EditPicture(int id)
        {
            var data = _pictureRepository.GetAsset(id);
            if (data == null) return NotFound();

            var vm = new EditPictureViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingImg = data.Img
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPicture(EditPictureViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var data = _pictureRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            data.Name = vm.Name;
            data.Topic = vm.Topic;

            if (vm.ImgFile != null)
            {
                _fileService.DeleteFile(data.Img, "images/pictures");
                var newFileName = await _fileService.SaveFileAsync(vm.ImgFile, "images/pictures");
                data.Img = newFileName;
            }

            _pictureRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllPictures));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManagePictures)
        // ===========================

        [HasPermission(Permission.CanManagePictures)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePicture(int id)
        {
            var data = _pictureRepository.GetAsset(id);
            if (data == null) return NotFound();

            _fileService.DeleteFile(data.Img, "images/pictures");
            _pictureRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllPictures));
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private PictureViewModel PassDataToViewModel(PictureData d)
        {
            return new PictureViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Topic = d.Topic,
                Img = d.Img
            };
        }
    }
}
