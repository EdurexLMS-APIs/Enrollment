using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class RegisteredUsersVM
    {
        public List<RegisteredUsers> RegisteredUsers { get; set; }
        public BlockUserDTO blockUserDTO { get; set; }
        public EditRegisteredUser EditRegisteredUser { get; set; }
        public ResetUserPasswordDTO ResetUserPasswordDTO { get; set; }
    }
    public class BlockUserDTO
    {
        public string userEmail { get; set; }
        public string adminEmail { get; set; }
        public string PIN { get; set; }
    }
    public class ResetUserPasswordDTO
    {
        public string userEmail { get; set; }
        public string adminEmail { get; set; }
        public string newPassword { get; set; }
        public string PIN { get; set; }
    }
    public class EditRegisteredUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RegisteredDate { get; set; }
        public UserRolesEnums Role { get; set; }
        public UserStatusEnums Status { get; set; }
        public DateTime LastLogin { get; set; }
    }
    public class RegisteredUsers
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RegisteredDate { get; set; }
        public string Role { get; set; }
        public UserStatusEnums Status { get; set; }
        public DateTime LastLogin { get; set; }
    }
    public class RegisteredStudentsVM
    {
        public List<RegisteredStudents> RegisteredStudents { get; set; }
        public BlockUserDTO blockStudentDTO { get; set; }
        public EditRegisteredUser EditRegisteredUser { get; set; }
        public ResetUserPasswordDTO ResetUserPasswordDTO { get; set; }
    }
    public class RegisteredStudents
    {
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RegisteredDate { get; set; }
        public int TotalCourses { get; set; } = 0;
        public int TotalCertifications { get; set; } = 0;
        public UserStatusEnums Status { get; set; }
        public DateTime LastLogin { get; set; }
    }
    public class RegisteredAffiliates
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RegisteredDate { get; set; }
        public int TotalReferred { get; set; } = 0;
        public string Role { get; set; }
    }
}
