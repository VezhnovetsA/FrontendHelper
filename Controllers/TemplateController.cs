using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using FrontendHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace FrontendHelper.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateConverter _converter;
        private readonly TemplateRepository _templateRepository;

        public TemplateController(ITemplateConverter converter, TemplateRepository templateRepository)
        {
            _converter = converter;
            _templateRepository = templateRepository;
        }

        // ===========================
        // ПРОСМОТР (CanViewTemplates)
        // ===========================

        [HasPermission(Permission.CanViewTemplates)]
        [HttpGet]
        public IActionResult ShowAllTemplates()
        {
            var vms = _converter.GetAllPreviews().ToList();
            return View(vms);
        }

        [HasPermission(Permission.CanViewTemplates)]
        [HttpGet]
        public IActionResult ShowTemplate(int id)
        {
            var vm = _converter.GetFullTemplate(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            return View(new CreateTemplateViewModel());
            // CreateTemplateViewModel: { string Name; IFormFile HtmlFile; }
        }

        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTemplate(CreateTemplateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Сохраняем HTML-файл в /wwwroot/templates
            using var stream = vm.HtmlFile.OpenReadStream();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.HtmlFile.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", fileName);
            using (var fs = new FileStream(path, FileMode.Create))
            {
                vm.HtmlFile.CopyTo(fs);
            }

            var entity = new TemplateData
            {
                Name = vm.Name,
                TemplateCode = fileName
            };
            _templateRepository.AddAsset(entity);
            return RedirectToAction(nameof(ShowAllTemplates));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult EditTemplate(int id)
        {
            var data = _templateRepository.GetTemplateById(id);
            if (data == null) return NotFound();

            var vm = new EditTemplateViewModel
            {
                Id = data.Id,
                Name = data.Name,
                ExistingCode = data.TemplateCode
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTemplate(EditTemplateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var data = _templateRepository.GetTemplateById(vm.Id);
            if (data == null) return NotFound();

            data.Name = vm.Name;
            if (vm.HtmlFile != null)
            {
                // Удаляем старый файл
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", data.TemplateCode);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                // Сохраняем новый
                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.HtmlFile.FileName)}";
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", newFileName);
                using (var fs = new FileStream(newPath, FileMode.Create))
                {
                    vm.HtmlFile.CopyTo(fs);
                }
                data.TemplateCode = newFileName;
            }

            _templateRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllTemplates));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTemplate(int id)
        {
            var data = _templateRepository.GetTemplateById(id);
            if (data == null) return NotFound();

            // Удаляем файл с диска
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", data.TemplateCode);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _templateRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllTemplates));
        }

        // ===========================
        // СКАЧАТЬ КОД (CanManageTemplates)
        // ===========================

        [HasPermission(Permission.CanManageTemplates)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var tpl = _templateRepository.GetTemplateById(id);
            if (tpl == null) return NotFound();

            var content = System.IO.File.ReadAllText(Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "templates", tpl.TemplateCode));
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"template_{tpl.Id}.html";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }
    }
}
