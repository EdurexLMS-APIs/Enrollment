using CPEA.Utilities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class AdminRolesVM
    {
        public List<AdminRolesdto> adminRolesList { get; set; }
        public AdminRolesdto createAdminRoles { get; set; }
    }
}
