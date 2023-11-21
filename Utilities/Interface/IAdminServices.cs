using CPEA.Models;
using CPEA.Utilities.DTO;
using CPEA.Utilities.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IAdminServices
    {
        Task<ResponseList<RegisteredStudents>> ManualEnrollment(ManualRegisterUserDTO dto, string url);
        Task<List<InstitutionType>> InstitutionTypes();
        Task<List<InstitutionRecord>> Institutions();
        Task<List<InstitutionRecord>> AddInstitution(AddInstitutionDTO dto, string LogoPath);
        Task<List<InstitutionRecord>> EditInstitution(InstitutionEditVM dto);
        Task<InstitutionDestOffVM> InstitutionDestOffList(int InstitutionId);
        Task<InstitutionDestOffVM> AddInstitutionDestOff(InstitutionDestOffAdd dto);
        Task<InstitutionDestOffVM> EditInstitutionDestOff(InstitutionDeskOfficerEdit dto);
        Task<InstitutionDestOffVM> DeleteInstitutionDestOff(int Id);
        Task<List<InstitutionRecord>> DeleteInstitution(int Id);
        Task<List<InstitutionRecord>> InstitutionStatus(int InstitutionId);
        Task<ReportVM> Reports(ReportDTO dto);
        Task<List<AdminRolesdto>> AdminRoles();
        Task<List<AdminRolesdto>> EditRole(AdminRolesdto dto);
        Task<List<AdminRolesdto>> AddRole(AdminRolesdto dto);
        Task<AdminDashboardVM> RegisterAdmin(AdminRegisVM dto);
        Task<DashboardVM> DashboardRe(string email);
        Task<List<object>> chartUserbyRole();
        Task<List<object>> chartEnrollmentbyQueryString(string param);
        Task<List<RegisteredUsers>> RegisteredUsers();
        Task<List<RegisteredUsers>> EditUser(EditUser dto);
        Task<List<RegisteredUsers>> BlockedUsers();
        Task<Tuple<List<RegisteredUsers>, Users, string>> BlockUser(BlockUserDTO dto);
        Task<Tuple<List<RegisteredUsers>, Users, string>> UnBlockUser(BlockUserDTO dto);
        Task<Tuple<List<RegisteredUsers>, Users, string>> ResetPassword(string email);
        Task<Tuple<List<RegisteredUsers>, Users, string>> ResetUserPassword(ResetUserPasswordDTO dto);
        Task<List<RegisteredStudents>> RegisteredStudents();
        Task<List<RegisteredAffiliates>> RegisteredAffiliates();
        Task<StudentDetails> SingleStudent(string email);
        Task<List<RegisteredStudents>> BlockedStudents();
        Task<List<RegisteredStudents>> SingleUserReferred(string email);
        Task<Tuple<List<RegisteredStudents>, Users, string>> BlockStudent(BlockUserDTO dto);
        Task<Tuple<List<RegisteredStudents>, Users, string>> UnBlockStudent(BlockUserDTO dto);
        Task<Tuple<List<RegisteredStudents>, Users, string>> ResetStudentPassword(ResetUserPasswordDTO dto);
        Task<List<RegisteredStudents>> Enrollment();
        Task<List<RegisteredStudents>> PartialEnrollment();
        Task<List<Programs>> Programs();
        Task<List<Currency>> Currencys();
        Task<List<Programs>> AddPrograms(Programs dto);
        Task<List<ProgramCategory>> ProgramCategoriesByInstitutionId(int InstitutionId);
        Task<List<ProgramCategory>> ProgramCategories();
        Task<List<ProgramCategory>> AddProgramCategory(ProgramCategory dto);
        Task<List<Cour>> Courses();
        Task<List<Cour>> EditCourse(EditCourseDTO dto);
        Task<CoursePriceOptionVM> CourseOptions(int CourseId);
        Task<CoursePriceOptionVM> EditCourseOption(EditCourseOptionDTO dto);
        Task<List<Cour>> AddCourse(Courses dto);
        Task<CoursePriceOptionVM> AddCoursePrice(CoursePriceOptions dto);
        Task<List<Certi>> Certifications();
        Task<CertPriceOptionVM> CertificationOptions(int CertId);
        Task<CertPriceOptionVM> AddCertificationPrice(CertificationPriceOptions dto);
        Task<List<Certi>> AddCertification(Certifications dto);

        //Task<StudentDetails> SingleEnrollment(string email);
        Task<PaymentVM> TransactionLog();
        Task<PaymentVM> Paymemts(); 
         Task<List<AllPayments>> PaymentbyUser(string userEmail);
        Task<AllPayments> SinglePayment(int paymentId);
        Task<string> ConfirmSinglePayment(ConfirmPaymentVM paymentId);
        Task<Tuple<Users, string>> Login(LoginDTO dto);
        Task<List<ProgramOptions>> ProgramOptions();
        Task<List<ProgramOptions>> AddProgramOption(ProgramOptions dto);
        Task<List<programFees>> ProgramFees();
        Task<string> Logout();

        //Task<programFees> ProgramFees(int programId);
    }
}
