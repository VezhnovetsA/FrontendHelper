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
    public class ButtonController : Controller
    {
        private readonly ButtonRepository _buttonRepository;
        private readonly IFileService _fileService;

        public ButtonController(ButtonRepository buttonRepository, IFileService fileService)
        {
            _buttonRepository = buttonRepository;
            _fileService = fileService;
        }

        // ===========================
        // ПРОСМОТР
        // ===========================
        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowAllButtons()
        {
            var data = _buttonRepository.GetAssets();
            var vm = data.Select(d => new ButtonViewModel
            {
                Id = d.Id,
                Name = d.Name,
                ButtonCode = d.ButtonCode,
                Topic = d.Topic
            }).ToList();
            return View(vm);
        }

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowAllButtonsOnTheTopic(string topic)
        {
            var data = _buttonRepository.GetButtonsByTopic(topic);
            var vm = data.Select(d => new ButtonViewModel
            {
                Id = d.Id,
                Name = d.Name,
                ButtonCode = d.ButtonCode,
                Topic = d.Topic
            }).ToList();
            ViewBag.Topic = topic;
            return View(vm);
        }

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowGroupsOfButtons(int numberPerGroup = 6)
        {
            var topics = _buttonRepository.GetButtonTopics();
            var vm = topics.Select(t =>
            {
                var list = _buttonRepository
                    .GetButtonsByTopic(t)
                    .Take(numberPerGroup)
                    .Select(d => new ButtonViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        ButtonCode = d.ButtonCode,
                        Topic = d.Topic
                    })
                    .ToList();

                return new ButtonGroupViewModel
                {
                    Topic = t,
                    Buttons = list
                };
            }).ToList();

            return View(vm);
        }

        // ===========================
        // СОЗДАНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult CreateButton()
        {
            return View(new CreateButtonViewModel());
        }

        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateButton(CreateButtonViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // проверяем расширение вручную
            var ext = Path.GetExtension(vm.HtmlFile.FileName)?.ToLowerInvariant();
            if (ext != ".html")
            {
                ModelState.AddModelError(nameof(vm.HtmlFile), "Допускаются только HTML-файлы (.html).");
                return View(vm);
            }

            var savedFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "buttons");
            var data = new ButtonData
            {
                Name = vm.Name,
                Topic = vm.Topic,
                ButtonCode = savedFileName
            };
            _buttonRepository.AddAsset(data);

            return RedirectToAction(nameof(ShowAllButtons));
        }

        // ===========================
        // РЕДАКТИРОВАНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult EditButton(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            var vm = new EditButtonViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Topic = data.Topic,
                ExistingFileName = data.ButtonCode
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditButton(EditButtonViewModel vm)
        {
            var data = _buttonRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            // сохраняем старое имя, чтобы его показать, если валидация упадёт
            vm.ExistingFileName = data.ButtonCode;

            // Убираем из ModelState поле ExistingFileName, чтобы оно не считалось «обязательным»
            ModelState.Remove(nameof(vm.ExistingFileName));

            if (!ModelState.IsValid)
                return View(vm);

            data.Name = vm.Name;
            data.Topic = vm.Topic;

            if (vm.HtmlFile != null)
            {
                var ext = Path.GetExtension(vm.HtmlFile.FileName)?.ToLowerInvariant();
                if (ext != ".html")
                {
                    ModelState.AddModelError(nameof(vm.HtmlFile), "Допускаются только HTML-файлы (.html).");
                    return View(vm);
                }

                _fileService.DeleteFile(data.ButtonCode, "buttons");
                var newFileName = await _fileService.SaveFileAsync(vm.HtmlFile, "buttons");
                data.ButtonCode = newFileName;
            }

            _buttonRepository.UpdateAsset(data);
            return RedirectToAction(nameof(ShowAllButtons));
        }

        // ===========================
        // УДАЛЕНИЕ (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteButton(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            _fileService.DeleteFile(data.ButtonCode, "buttons");
            _buttonRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowAllButtons));
        }

        // ===========================
        // СКАЧАТЬ КОД (CanManageButtons)
        // ===========================
        [HasPermission(Permission.CanManageButtons)]
        [HttpGet]
        public IActionResult DownloadCode(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "buttons", data.ButtonCode);
            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            var contentBytes = System.IO.File.ReadAllBytes(physicalPath);
            var fileName = $"button_{data.Id}.html";
            return File(contentBytes, "text/html; charset=utf-8", fileName);
        }
    }
}
