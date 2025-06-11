using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    [IsAdmin]
    public class UserController : Controller
    {
        private static readonly HashSet<string> _undeletableRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "User",
        "PremiumUser"

    };

        private UserRepository _userRepository;
        private RoleRepository _roleRepository;

        public UserController(RoleRepository roleRepository, UserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexUserViewModel();
            viewModel.Users = _userRepository
                  .WithRoles()  
                  .Select(u => new UserViewModel
                  {
                      Id = u.Id,
                      Name = u.UserName,
                      RoleId = u.RoleId
                  })
                  .ToList();


            viewModel.Roles = _roleRepository.GetAssets().Select(x => new RoleViewModel()
            {
                Id = x.Id,
                Name = x.RoleName
            })
                .ToList();

            viewModel.Roles.Add(new RoleViewModel
            {
                Id = null,
                Name = "Нет роли"
            });


            return View(viewModel);
        }

        public IActionResult UpdateUserRole(int id, int roleId)
        {
            _userRepository.UpdateRole(id, roleId);
            return RedirectToAction("Index");
        }



        public IActionResult ShowRoles()
        {
            var viewModel = new RolesViewModel();
            viewModel.Roles = _roleRepository.GetAssets().Select(x => new RoleWithPermissionsViewModel
            {
                Id = x.Id,
                Name = x.RoleName,
                Permissions = GetPermissions(x.Permission),
            })
                .ToList();

            viewModel.Permissions = Enum.
                GetValues<Permission>()
                .Select(x => new PermissionViewModel
                {
                    Permission = x,
                    PermissionName = GetDisplayName(x),
                })
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateRole(string roleName)
        {
            var role = new RoleData
            {
                RoleName = roleName,
            };

            _roleRepository.AddAsset(role);

            return RedirectToAction("ShowRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRole(int id)
        {
            var role = _roleRepository.GetAsset(id);
            if (_undeletableRoles.Contains(role.RoleName))
            {
                TempData["Error"] = $"Роль «{role.RoleName}» нельзя удалить.";
                return RedirectToAction(nameof(ShowRoles));
            }

            _roleRepository.RemoveAsset(id);
            return RedirectToAction(nameof(ShowRoles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRole(int id, List<Permission> permissions)
        {
            var role = _roleRepository.GetAsset(id);
            if (string.Equals(role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Права для роли «Admin» менять нельзя.";
                return RedirectToAction(nameof(ShowRoles));
            }

            _roleRepository.UpdatePermission(id, permissions);
            return RedirectToAction(nameof(ShowRoles));
        }



        private List<Permission> GetPermissions(Permission rolePermission)
        {
            return Enum
                .GetValues<Permission>()
                .Where(availablePermission => rolePermission.HasFlag(availablePermission))
                .ToList();
        }


        private string GetDisplayName(Permission permission)
        {
            return Enum.GetName<Permission>(permission);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            _userRepository.RemoveAsset(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
