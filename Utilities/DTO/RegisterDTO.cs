using CPEA.Utilities.InterswitchClasses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class ManualRegisterUserDTO
    {
        public string accountType { get; set; }
        public PersonalReg personalReg { get; set; }
        public BusinessReg businessReg { get; set; }
        public string NYSCCallUpNumber { get; set; }
        public int CoursePriceOptionId { get; set; }
        public int CertificateTypeId { get; set; } = 0;
        public int deliveryStateId { get; set; } = 0;
        public float AmountPaid { get; set; }
        public int CertificationPriceOptionId { get; set; }
        public string PaymentMethod { get; set; }
        public string OfflinePaymentRef { get; set; }
        //public string PromoCode { get; set; }
    }
    public class RegisterDTO
    {
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public string Address { get; set; }
        //public int StateId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public int ProgramId { get; set; }
       
        public int PaymentMethod { get; set; }
        public List<int> SubjectIds { get; set; }
        public Choices Choices { get; set; }

        public string paymentDeposit { get; set; } 
       
        public string Gender { get; set; }
        public string referralCode { get; set; }
        public float Amount { get; set; }
        public int ProgramOptionId { get; set; }
        [Required]
        public string heardAboutUs { get; set; }
    }
    public class RegisterUserDTO
    {
        public string accountType { get; set; }
        public PersonalReg personalReg { get; set; }
        public BusinessReg businessReg { get; set; }
        public string NYSCCallUpNumber { get; set; }
        //public string PromoCode { get; set; }
    }
    public class PersonalReg
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string AlternatePhone { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
       // public string accountType { get; set; }

        //[RegularExpression(@"^(\d|\w)+$", ErrorMessage = "Space and Characters Not Allowed")]
        [Remote(action: "IsUsernameInUse", controller: "Enrollment")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string referralCode { get; set; }
    }
    public class BusinessReg
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string AlternatePhone { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9]*$|^[^\s]", ErrorMessage = "Space and Characters Not Allowed")]
        [Remote(action: "IsUsernameInUse", controller: "EnrollmentAccount")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string BusinessEmail { get; set; }
        public string BusinessAddress { get; set; }
        public int BusinessCityId { get; set; }
        public string UserRole { get; set; }
        //public string accountType { get; set; }

    }
    public class Register2DTO
    {
        public string userId { get; set; }
        public Register2DataOption userDataOption { get; set; }
        public Register2CourseOption userCourseOption { get; set; }
        public Register2CertificationOption userCertificationOption { get; set; }
        public Register2ModemOption userModemOption { get; set; }
        public Register2PaymentOption UserPayment { get; set; }
        public string PromoCode { get; set; }
        public string refCode { get; set; }
    }
    public class Register2CourseOption
    {
        public int CoursePriceOptionId { get; set; }
        public int CertificateTypeId { get; set; } = 0;
        public int deliveryStateId { get; set; } = 0;
        public float AmountPaid { get; set; }
    }
    public class Register2CertificationOption
    {
        public int CertificationPriceOptionId { get; set; }
    }
    public class Register2DataOption
    {
        public int DataId { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class Register2ModemOption
    {
        public int ModemId { get; set; }
    }
    public class Register2PaymentOption
    {
        public float totalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string OfflinePaymentRef { get; set; }
    }
    public class Choices
    {
        public int FirstInstitution { get; set; }
        public int SecondInstitution { get; set; }
        public int FirstCourse { get; set; }
        public int SecondCourse { get; set; }

    }

    public class EnrollmentUser
    {
        public int ProgramId { get; set; }
        public int CategoryId { get; set; }
        public int CourseId { get; set; }
        public int? CoursePriceOptionId { get; set; }
        public int? CertPriceOptionId { get; set; }
    }
    public class PasswordResetDTO
    {
       public string NewPassword { get; set; }
       public string ConfirmPassword { get; set; }
       public string email { get; set; }
    }
}
