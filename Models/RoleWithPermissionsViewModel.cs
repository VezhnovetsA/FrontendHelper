using FhEnums;

namespace FrontendHelper.Models
{
    public class RoleWithPermissionsViewModel
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public List<Permission> Permissions { get; set; }   
    }
}
