using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendHelper.Controllers
{
    public class ButtonController : Controller
    {
        private readonly ButtonRepository _buttonRepository;

        public ButtonController(ButtonRepository buttonRepository)
        {
            _buttonRepository = buttonRepository;
        }

        // ===========================
        // ПРОСМОТР (CanViewButtons)
        // ===========================

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowAllButtons()
        {
            var data = _buttonRepository.GetAssets();
            var vm = data.Select(PassDataToViewModel).ToList();
            return View(vm);
        }

        [HasPermission(Permission.CanViewButtons)]
        public IActionResult ShowButton(int id)
        {
            var data = _buttonRepository.GetAsset(id);
            if (data == null) return NotFound();
            var vm = PassDataToViewModel(data);
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
            // CreateButtonViewModel: { string Name; [Required] string ButtonCode; }
        }

        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateButton(CreateButtonViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var data = new ButtonData
            {
                Name = vm.Name,
                ButtonCode = vm.ButtonCode
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
                ButtonCode = data.ButtonCode
            };
            return View(vm);
        }

        [HasPermission(Permission.CanManageButtons)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditButton(EditButtonViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var data = _buttonRepository.GetAsset(vm.Id);
            if (data == null) return NotFound();

            data.Name = vm.Name;
            data.ButtonCode = vm.ButtonCode;
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
            var content = data.ButtonCode;
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"button_{data.Id}.html";
            return File(bytes, "text/plain; charset=utf-8", fileName);
        }

        // ===========================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===========================

        private ButtonViewModel PassDataToViewModel(ButtonData d)
        {
            return new ButtonViewModel
            {
                Id = d.Id,
                Name = d.Name,
                ButtonCode = d.ButtonCode
            };
        }
    }
}
