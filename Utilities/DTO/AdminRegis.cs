using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class AdminRegisVM
    {
       public AdminRegis AdminRegis { get; set; }
       public IEnumerable<AdminRolesdto> adminRoles { get; set; }
    }
    public class AdminRegis
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Phone { get; set; } 
        [Required]
        public int Role { get; set; }
        //public UserRolesEnums Role { get; set; }
    }
}
