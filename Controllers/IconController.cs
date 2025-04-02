//using FrontendHelper.interfaces;
//using Microsoft.AspNetCore.Mvc;

//namespace FrontendHelper.Controllers
//{
//    public class IconController : Controller
//    {
//        private readonly IAllIcons _iconRepository;

//        public IconController(IAllIcons iconRepository)
//        {
//            _iconRepository = iconRepository;
//        }

//        public IActionResult IconList()
//        {
//            var icons = _iconRepository.Icons;
//            return View(icons);
//        }

//    }
//}

using FrontendHelper.interfaces;
using Microsoft.AspNetCore.Mvc;

public class IconController : Controller
{
    private readonly IAllIcons _iconRepository;

    public IconController(IAllIcons iconRepository)
    {
        _iconRepository = iconRepository;
    }

    public IActionResult IconList()
    {
        var icons = _iconRepository.Icons;
        return View(icons);
    }

    // Новый экшн для скачивания иконки
    public IActionResult DownloadIcon(int id)
    {
        var icon = _iconRepository.GetIcon(id);

        if (icon == null)
        {
            return NotFound();
        }

        var relativePath = icon.Img;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));

        // проверка существования файла
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Файл не найден.");
        }

        // чтение
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var fileName = Path.GetFileName(filePath);

        return File(fileBytes, "application/octet-stream", fileName);
    }
}
