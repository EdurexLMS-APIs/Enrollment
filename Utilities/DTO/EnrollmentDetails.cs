using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class EnrollmentDetails
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string ReferralCode { get; set; }
        public string Address { get; set; }
        public UserStatusEnums Status { get; set; }
        public string RegisteredDate { get; set; }
        public string Gender { get; set; }
        public UserRolesEnums Role { get; set; }
    }

    public class StudentDetails
    {
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserStatusEnums Status { get; set; }
        public string RegisteredDate { get; set; }
        public List<StudentCourses> StCourses { get; set; }
        public List<StudentCertifications> StCerts { get; set; }

    }
    public class StudentCourses
    {
        public int CourseId { get; set; }
        public string Program { get; set; }
        public string Category { get; set; }
        public string CourseName { get; set; }
        public string OptionName { get; set; }
        public string Price { get; set; }
        public string AmountPaid { get; set; }
        public UserProgramPaymentStatusEnums PaymentStatus { get; set; }
        //public string StartDate { get; set; }
        public string RegisteredDate { get; set; }
        public UserCourseStatusEnums CourseStartStatus { get; set; }
        public string CompletedDate { get; set; }
        public bool CertificateIssued { get; set; } 
        public string CertificateIssuedDate { get; set; }
    }
    public class StudentCertifications
    {
        public int CertificationId { get; set; }
        public string Program { get; set; }
        public string Category { get; set; }
        public string ExamDate { get; set; }
        public string Price { get; set; }
        public string AmountPaid { get; set; }
        public UserProgramPaymentStatusEnums PaymentStatus { get; set; }
        public string Name { get; set; }
        public CertificationTypeEnums Mode { get; set; }
        public string OrganisationName { get; set; }
        public string ShortCode { get; set; }
       // public int AmountDollar { get; set; }
        public string RegisteredDate { get; set; }
        public UserCourseStatusEnums CourseStartStatus { get; set; }
        public string CompletedDate { get; set; }
        public bool CertificateIssued { get; set; }
        public string CertificateIssuedDate { get; set; }
        public string Currency { get; set; }
    }
}
