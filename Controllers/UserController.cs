using FHDatabase.Models;
using FHDatabase.Repositories;
using FhEnums;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Controllers
{
    public class UserController : Controller
    {
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
                .GetUsersWithRole()
                .Select(x => new UserViewModel
            {
                Id = x.Id,
                Name = x.UserName,
                RoleId = x.Role?.Id

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

        public IActionResult UpdateUserRole(int id, int? roleId)
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

        public IActionResult DeleteRole(int id)
        {

            _roleRepository.RemoveAsset(id);

            return RedirectToAction("ShowRoles");
        }

        public IActionResult UpdateRole(int id, List<Permission> permissions)
        {

            _roleRepository.UpdatePermission(id, permissions);
            return RedirectToAction("ShowRoles");
        }


        //move
        private List<Permission> GetPermissions(Permission rolePermission)
        {
            return Enum
                .GetValues<Permission>()
                .Where(availablePermission => rolePermission.HasFlag(availablePermission))
                .ToList();
        }

        //move
        private string GetDisplayName(Permission permission)
        {
            return Enum.GetName<Permission>(permission);

        }

    }
}
