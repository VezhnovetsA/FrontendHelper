using FHDatabase.Models;
using FHDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class FilterController : Controller
    {
        private readonly FilterRepository _filterRepository;
        public FilterController(FilterRepository repo) => _filterRepository = repo;




        public IActionResult Index() =>
           View(_filterRepository.GetAssets().ToList());




        [HttpGet]
        public IActionResult Create() => View(new FilterData());




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FilterData f)
        {
            if (!ModelState.IsValid) return View(f);
            _filterRepository.AddAsset(f);
            return RedirectToAction(nameof(Index));
        }

    }
}
