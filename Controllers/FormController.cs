using FHDatabase.Models;
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
    public class FormController : Controller
    {
        private readonly FormRepository _formRepository;
        private readonly IFileService _fileService;

        public FormController(FormRepository formRepository, IFileService fileService)
        {
            _formRepository = formRepository;
            _fileService = fileService;
        }

        // ===========================
        // ПРОСМОТР (CanViewForms)
        // ===========================

        [HasPermission(Permission.CanViewForms)]
        public IActionResult ShowAllForms()
        {
            var data = _formRepository.GetAssets();
            var vms = data.Select(PassDataToViewModel).ToList();
            return View(vms);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageForms)
        // ===========================

        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult CreateForm()
        {
            return View(new CreateFormViewModel());
            // CreateFormViewModel: { string Name; IFormFile FormFile (HTML); }
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForm(CreateFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Сохраняем HTML-файл в /wwwroot/forms
            using var stream = vm.FormFile.OpenReadStream();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.FormFile.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forms", fileName);
            using (var fs = new FileStream(path, FileMode.Create))
                vm.FormFile.CopyTo(fs);

            var entity = new FormData
            {
                Name = vm.Name,
                FormCode = fileName
            };
            _formRepository.AddAsset(entity);
            return RedirectToAction(nameof(ShowAllForms));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageForms)
        // ===========================

        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult EditForm(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();

            var vm = new EditFormViewModel
            {
                Id = data.Id,
                Name = data.Name,
                ExistingCode = data.FormCode
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageForms)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditForm(EditFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var data = _formRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            data.Name = vm.Name;
            if (vm.FormFile != null)
            {
                // Удаляем старый файл
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forms", data.FormCode);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                // Сохраняем новый
                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.FormFile.FileName)}";
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forms", newFileName);
                using var fs = new FileStream(newPath, FileMode.Create);
                vm.FormFile.CopyTo(fs);
                data.FormCode = newFileName;
            }

            _formRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllForms));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageForms)
        // ===========================

        [HasPermission(Permission.CanManageForms)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteForm(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();

            // Удаляем файл
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "forms", data.FormCode);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _formRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllForms));
        }

        // ===========================
        // СКАЧАТЬ КОД (CanManageForms)
        // ===========================

        [HasPermission(Permission.CanManageForms)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var data = _formRepository.GetAsset(id);
            if (data == null) return NotFound();

            var content = System.IO.File.ReadAllText(Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "forms", data.FormCode));
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"form_{data.Id}.html";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private FormViewModel PassDataToViewModel(FormData d)
        {
            return new FormViewModel
            {
                Id = d.Id,
                Name = d.Name,
                FormCode = d.FormCode
            };
        }
    }
}
