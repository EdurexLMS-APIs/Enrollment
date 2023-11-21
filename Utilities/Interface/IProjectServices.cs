using CPEA.Models;
using CPEA.Utilities.DTO;
using CPEA.Utilities.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IProjectServices
    {
        Task<Tuple<string,bool>> ForgotPassword(string email, string url);
        Task<string> ResetPassword(PasswordResetDTO dto);
        Task<Tuple<CourseRegConfirmDTO, string>> SendRegConfirmation(string userId, int userCourseId, string Mode);
        CourseRegConfirmDTO DownloadRegConfirmation(string userId, int userCourseId, string Mode);
        Task<string> GetPromoCode(string Code);
        Task<List<UserRequest>> GetRequest(string userId);
        Task<UserRequest> SingleRequest(int Id, string userId);
        Task<List<UserRequest>> SendRequest(UserRequest dto, string userId);
        Task<Response<Users>> Enrollment(RegisterUserDTO dto, string url);
        Task<WalletRecord> GetWallet(string UserId);
        Task<bool> CreateWallet(UserWallet dto);
        Task<PaymentInitialiseResponse> Enrollment2(Register2DTO dto);
        Task<string> EnrollmentInterswitch(Register2DTO dto);

        Task<Register1VM> GetRegister();
        Task<RegisterNewVM> GetRegisterNew(string userId);
        Task<List<CertificateType>> GetCertificateType();
        Task<string> GetCertificateFeeByTypeId(int TypeId);
        Task<List<Programs>> GetPrograms();
        Task<List<ProgramOptions>> GetProgramOptionsByCategoryId(int CategoryId);
        //Task<List<Programs>> GetProgramsByCategoryId(int CategoryId);
        //Task<List<ProgramOptions>> GetProgramOptionsByProgramId(int ProgramId);
        Task<List<Subjects>> GetProgramSubjectssByOptionId(int OptionId);
        Task<List<Countries>> GetCountries();
        Task<List<States>> GetStatesByCountryId(int CountryId);
        Task<List<Cities>> GetCitiesByStateId(int StateId);
        Task<string> GetCourierFeeByStateId(int StateId);
        Task<string> GetAddressByCityId(int CityId);
        //Task<List<Streets>> GetStreetsByCityId(int CityId);
        Task<List<Institutions>> GetInstitutions();
        Task<List<Courses>> GetCoursesbyProgramCat(int ProgramCatId);
        Task<List<CourseOptionDates>> GetCourseOptionsDatesbyCourseId(int CourseId);
        Task<List<CourseOptionDetails>> GetCourseOptionsbyOptionDate(string OptionDate, int selectedCourseId);
        Task<List<Certifications>> GetCertificatesbyCourseId(int CourseId);
        Task<List<Certifications>> GetCertificatesbyCategoryId(int CategoryId);
        Task<List<CertificateOptionDates>> GetCertificatesOptionsbyId(int CertId);
        Task<string> GetCertificationConvertedValuebyCertOptId(int CertOptId);

        //Task<string> GetPriceByProgramId(int ProgramId);
        Task<string> GetPriceByProgramOptionId(int ProgramOptionId);
        Task<PaymentInitialiseResponse> InitializeCardPayment(PaymentDTO dto, PaymentMethodEnums paymentMethod, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeAccountTransferPayment(PaymentDTO dto, PaymentMethodEnums paymentMethod, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeBankCOnnectPayment(PaymentDTO dto, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeOfflinePayment(PaymentDTO dto, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeCardPayment2(List<PaymentDTO2> dto, PaymentMethodEnums paymentMethod, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeOfflinePayment2(List<PaymentDTO2> dto, string redirectURL);
        Task<PaymentInitialiseResponse> InitializeInterSwitchPayment2(List<PaymentDTO2> dto);

        Task<PaymentInitialiseResponse> RegisterUser(RegisterDTO dto);
        Task<string> Logout();
        Task<Tuple<Users, string, string>> Login(LoginDTO dto);
        Task<StudentDashboardVM> DashboardRe(string userId, string IP4);
        Task<Users> GetProfile(string userId);
        Task<List<UserCourseRecord>> StudentCourses(string userId);
        Task<UserCourseRecord> SingleCourse(int Id, string userId);
        Task<List<UserCertificationsRecord>> StudentCertifications(string userId);
        Task<UserDevicesSubscriptionsVM> StudentDataDevices(string userId);
        Task<PaymentInitialiseResponse> ChangeCourseDate(ChangeCDate dto, string redirectURL);


        Task<string> QueryBankCOnnectPayment(string PaymentRef);
        //Task<List<ProgramRecord>> UserPrograms(string email);
        Task<List<PaymentRecord>> UserPaymentHistories(string userId);
        Task<List<PaymentRecord>> EachPaymentHistories(string userId, string PaymentRef);
        GenInvoice GenerateInvoice(string userId, string PaymentRef);
        Task<List<UserCertificates>> UserCertificates(string email);
        Task<string> NewProgram(DashboardVM dto, string email);
        Task<List<ProgramCategory>> GetProgramCatByProgramId(int ProgramId);
        Task<string> DeleteUserProgramOption(int Id);
        Task<ReferralUsage> UserReferralCodeUsage(string email);
        Task<ListDiscountHisto> DiscountHistory(string code);
        Task<string> GetDiscountRate(string code);

    }
}
