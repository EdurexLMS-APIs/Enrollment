using CPEA.Models;
using CPEA.Utilities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class StudentDashboardVM
    {
        public string fullName { get; set; }
        public string studentNumber { get; set; }
        public UserCoursesVM UserCoursesVM { get; set; }
        //public List<UserCourseRecord> UserCoursesList { get; set; }
        public List<UserCertificationsRecord> UserCertificationsList { get; set; }
        public List<UserDataRecord> UserDataList { get; set; }
        public List<UserDevicesRecord> UserDevicesList { get; set; }
        public List<PaymentRecord> PaymentRecord { get; set; }
        public string SourceView { get; set; }
        public int CourseIdConfirmation { get; set; } = 0;
        public ChangeCDate ChangeCDateDTO { get; set; }
        public IEnumerable<Programs> programListz { get; set; }

    }
    public class ChangeCDate
    {
        public PaymentDTO2 paymentDTO { get; set; }
        public string userId { get; set; }
        public string redirectURL { get; set; }
        public int UserCourseId { get; set; }
        public int NewCourseOptionId { get; set; }

    }
    public class UserCoursesVM
    {        
        public List<UserCourseRecord> UserCourseList { get; set; }
        //public string SourceView { get; set; }
        public UserCourses newUserCourseDTO { get; set; }

    }
    public class UserCertificationsVM
    {
        public List<UserCertificationsRecord> UserCertificationList { get; set; }
        public string SourceView { get; set; }
        public UserCertifications newUserCertificationDTO { get; set; }

    }
    public class UserProfileVM
    {
        public Users User { get; set; }
        public string SourceView { get; set; }
        public UserEdit editUserDTO { get; set; }
    }
    public class UserEdit
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string AlternatePhone { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class UserPaymentsVM
    {
        public List<PaymentRecord> UserPaymentList { get; set; }
        public string SourceView { get; set; }
        //public UserCertifications newUserCertificationDTO { get; set; }
    }
    public class UserDevicesSubscriptionsVM
    {
        public List<UserDataRecord> UserSubscriptionList { get; set; }
        public List<UserDevicesRecord> UserDevicesList { get; set; }
        public string SourceView { get; set; }
        //public UserCertifications newUserCertificationDTO { get; set; }
    }
    public class DashboardVM
    {
        public TotalRecord today { get; set; }
        public TotalRecord thisWeek { get; set; }
        public TotalRecord thisMonth { get; set; }
        public string totalPrograms { get; set; }
        public string totalCategories { get; set; }
        public string totalCourses { get; set; }
        public string totalCertifications { get; set; }
        public TotalStudentRecord studentBlock { get; set; }
        public List<object> pieChart { get; set; }
        public List<object> doughNutChart { get; set; }
        public List<RecentLoginRecord> logRecord { get; set; }
        public string fullName { get; set; }
        public string programCategpryName { get; set; }
       // public List<ProgramRecord> ProgramCategorys { get; set; }
        public List<PaymentRecord> PaymentRecord { get; set; }
        public List<string> SubjectRecord { get; set; }
        public List<ChoicesRecord> ChoicesRecord { get; set; }
        public PaymentDTO paymentDTO { get; set; }
        public string SourceView { get; set; }
        public List<Programs> programListz { get; set; }
        public int programOptionId { get; set; }
        public string rCode { get; set; }
        public List<int> SubjectIds { get; set; }
        public string deleteSource { get; set; }
    }
    public class TotalRecord
    {
        public string RegisteredStudents { get; set; }
        public string WithoutCourses { get; set; }
        public string WithCourses { get; set; }
        public string AmountPaid { get; set; }
    }
    public class TotalStudentRecord
    {
        public string RegisteredStudents { get; set; }
        public string WithoutCourses { get; set; }
        public string WithCourses { get; set; }
        public string BlockedStudent { get; set; }
    }
    public class ChartRecord
    {
        public string Value { get; set; }
        public int Total { get; set; }
    }
    public class RecentLoginRecord
    {
        public string email { get; set; }
        public string fullName { get; set; }
        public DateTime log { get; set; }
        public string Ip { get; set; }
    }
    public class PaymentRecord
    {
        public string paymentForName { get; set; }
        public string Amount { get; set; }
        public PaymentStatusEnums Status{ get; set; }
        public PaymentMethodEnums PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentRef { get; set; }
        public paymentForEnums paymentfor { get; set; }
        public int paymentForId { get; set; }
    }
    public class GenInvoice
    {
        public List<PaymentRecord> PaymentRecord { get; set; }
        public string paymentRef { get; set; }
        public string ReceiptNum { get; set; }
        public string totalPayment { get; set; }
        public string paymentDate { get; set; }
        public Institutions Institutions { get; set; }
        public Users users { get; set; }
        public UserProgramPaymentStatusEnums CoursePaymentStatus { get; set; }
    }
    public class ChoicesRecord
    {
        public string Institution { get; set; }
        public string Course { get; set; }

    }
    public class UserCourseRecord
    {
        public string CourseCode { get; set; }
        public int CourseId { get; set; }
        public int userCourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Program { get; set; }
        public string InstitutionCode { get; set; }
        public string Institutio { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Price { get; set; }
        public string amountPaid { get; set; }
        public string amountOwing { get; set; }
        public string certificateType { get; set; }
        public string CourierState { get; set; }
        public UserCourseStatusEnums ProgramStatus { get; set; }
        public UserProgramPaymentStatusEnums ProgramPaymentStatus { get; set; }
        public string CourseOption { get; set; }
        public string Duration { get; set; }
        public string RegDate { get; set; }
        public bool CertIssued { get; set; }

    }
    public class UserCertificationsRecord
    {
        public CertificationTypeEnums CertificationMode { get; set; }
        public int userCertificationId { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public string ExamDate { get; set; }
        public string Price { get; set; }
        public int CertOptId { get; set; }
        public string currency { get; set; }
        public string provider { get; set; }
        public UserCourseStatusEnums ProgramStatus { get; set; }
        public UserProgramPaymentStatusEnums ProgramPaymentStatus { get; set; }
    }
    public class UserDataRecord
    {
        public string NetworkProvider { get; set; }
        public string Validity { get; set; }
        public string Bundle { get; set; }
        public string Amount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
    }
    public class UserDevicesRecord
    {
        public int UserDeviceId { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Amount { get; set; }
        public string PurchaseDate { get; set; }
    }
    public class ReferralUsage
    {
        public string rCode { get; set; }
        public List<ReferralUsage2> ReferralUsage2 { get; set; }

        public List<UserDiscount> Discount { get; set; }
    }
    public class ReferralUsage2
    {
        public string Fullname { get; set; }
        public string Program { get; set; }
        public string AmountPaid { get; set; }
        public string Earnings { get; set; }
        public string DateRegistered { get; set; }
    }
    public class DiscountHisto
    {
        public string Fullname { get; set; }
        public string Program { get; set; }
        public string ProgramFee { get; set; }
        public string Earnings { get; set; }
        public string DateRegistered { get; set; }
    }
    public class ListDiscountHisto
    {
        public string code { get; set; }
        public List<DiscountHisto> DiscountHisto { get; set; }

    }

    public class WalletRecord
    {
        public string WalletId { get; set; }
        public string AvailableBalance { get; set; }
        public List<WalletRecordList> WalletRecordList { get; set; }
    }
    public class WalletRecordList
    {
        public string Amount { get; set; }
        public WalletEnums Type { get; set; }
        public string PaymentDate { get; set; }
    }
}
