using Microsoft.AspNetCore.Identity;

namespace CPEA.Models
{
    public class UserRoles : IdentityUserRole<string>
    {
        public string StudentId { get; set; }
    }
}
