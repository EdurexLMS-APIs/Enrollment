using CPEA.Data;
using CPEA.Models;
using CPEA.Utilities.DTO;
using CPEA.Utilities.Interface;
using CPEA.Utilities.ViewModel;
using iTextSharp.text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace CPEA.Utilities.Services
{
    public class AdminServices : IAdminServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IProjectServices _projectServices;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMessagingService _messagingService;
        private readonly SignInManager<Users> _signInManager;
        private IWebHostEnvironment _env;
        private readonly IEmail _smtpMail;

        public AdminServices(IEmail smtpMail, SignInManager<Users> signInManager, IWebHostEnvironment env, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<Users> userManager, IMessagingService messagingService, IProjectServices projectServices)
        {
            _context = context;
            _userManager = userManager;
            _projectServices = projectServices;
            _messagingService = messagingService;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _env = env;
            _smtpMail = smtpMail;
        }

        public async Task<List<RegisteredAffiliates>> RegisteredAffiliates()
        {
            try
            {

                var result = await (from ur in _context.UserRoles
                                  join u in _userManager.Users on ur.UserId equals u.Id
                                  join r in _context.Role on ur.RoleId equals r.Id
                                  where r.Name != "Student" && r.Name != "Admin"
                                  select new RegisteredAffiliates
                                  {
                                      FirstName = u.FirstName,
                                      LastName =u.LastName,
                                      RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                      Role = r.Name,
                                      Email = u.Email,
                                      Phone =u.PhoneNumber,
                                      TotalReferred = _context.UserReferred.Where(m => m.ReferralId == u.Id).Count()
                                  }).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredStudents>> SingleUserReferred(string email)
        {
            try
            {
                var result = await (from ur in _context.UserReferred
                                    join r in _userManager.Users on ur.ReferralId equals r.Id
                                    join ru in _userManager.Users on ur.ReferredUserId equals ru.Id

                                    where r.Email == email

                                    select new RegisteredStudents
                                    {
                                        FirstName = ru.FirstName,
                                        LastName = ru.LastName,
                                        RegisteredDate = ru.RegisteredDate.ToString("dd/MM/yyyy"),
                                        Status = ru.Status,
                                        StudentNumber = ru.StudentNumber,
                                        Email = ru.Email,
                                        Phone = ru.PhoneNumber,
                                        TotalCourses = _context.UserCourses.Where(x => x.UserId == ru.Id).Count(),
                                        TotalCertifications = _context.UserCertifications.Where(x => x.UserId == ru.Id).Count(),
                                        LastLogin = _context.LoginTrail.Where(x => x.UserId == ru.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                    }

                                   ).ToListAsync();
                //var result = await _context.UserReferred.Include(x=>x.ReferredUser).Include(x=>x.Referral).Where(x => x.Referral.Email == email).ToListAsync();
                //var enrollmentList = new List<RegisteredStudents>();
                //if (result.Count() > 0)
                //{
                //    foreach (var item in result)
                //    {
                //        var enrollment = new RegisteredStudents
                //        {
                //            FirstName = item.ReferredUser.FirstName,
                //            LastName = item.ReferredUser.LastName,
                //            RegisteredDate = item.ReferredUser.RegisteredDate.ToString("dd/MM/yyyy"),
                //            Status = item.ReferredUser.Status,
                //            StudentNumber = item.ReferredUser.StudentNumber,
                //            Email = item.ReferredUser.Email,
                //            Phone = item.ReferredUser.PhoneNumber,
                //            TotalCourses = _context.UserCourses.Where(x => x.UserId == item.ReferredUser.Id).Count(),
                //            TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.ReferredUser.Id).Count(),
                //            LastLogin = _context.LoginTrail.Where(x => x.UserId == item.ReferredUser.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                //        };

                //        enrollmentList.Add(enrollment);
                //    }

                //}
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateStudentId()
        {
            Random rd = new Random();

            string result = "";

            for (int i = 0; i < 6; i++)
            {
                result += rd.Next(10);
            }


            result = "A-23" + result;

            return result;
        }
        public string ZeroPadUp(string value, int maxPadding, string prefix = null)
        {
            string result = value.PadLeft(maxPadding, '0');
            if (!string.IsNullOrEmpty(prefix)) { return prefix + result; }
            return result;
        }
        public async Task<ResponseList<RegisteredStudents>> ManualEnrollment(ManualRegisterUserDTO dto, string url)
        {
            try
            {

                var studentRoleId = await _roleManager.FindByNameAsync("Student");
                if (dto.accountType.ToLower() == "personal")
                {
                    var usernameExist = await _context.users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    if (usernameExist != null)
                    {
                        var res = await RegisteredStudents();
                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Email exist", Successful = false };
                    }

                    var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    if (currentuser != null)
                    {
                        var res = await RegisteredStudents();
                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Phone number exist.", Successful = false };

                    }

                    var user = new Users
                    {
                        FirstName = dto.personalReg.FirstName,
                        LastName = dto.personalReg.LastName,
                        MiddleName = dto.personalReg.MiddleName,
                        CityId = dto.personalReg.CityId,
                        Email = dto.personalReg.Email.ToLower(),
                        Address = dto.personalReg.Address,
                        Gender = dto.personalReg.Gender,
                        AlternatePhone = dto.personalReg.AlternatePhone,
                        PhoneNumber = dto.personalReg.PhoneNo,
                        UserName = dto.personalReg.Username.ToLower(),
                        StudentNumber = GenerateStudentId(),
                        Status = UserStatusEnums.Active,
                       // RoleId = studentRoleId,
                        RegisteredDate = DateTime.Now,
                        StaffDep = StaffDepEnums.None
                    };

                    var createdUser = await _userManager.CreateAsync(user, dto.personalReg.Password);

                    if (createdUser.Succeeded)
                    {                       
                        var webRoot = _env.WebRootPath;

                        var pathToFile = _env.WebRootPath
                                        + Path.DirectorySeparatorChar.ToString()
                                        + "Emails"
                                        + Path.DirectorySeparatorChar.ToString()
                                        + "welcome.html";

                        var builder = new StringBuilder();

                        using (var reader = File.OpenText(pathToFile))
                        {
                            builder.Append(reader.ReadToEnd());
                        }
                        string query = $"/Home/Login";
                        var fullUrl = url + query;


                        builder.Replace("{StudentName}", $"{user.FirstName} {user.LastName}");
                        builder.Replace("{StudentId}", user.StudentNumber);
                        builder.Replace("{Url}", fullUrl);

                        string messageBody = builder.ToString();
                        //Send Email
                        //----------
                        await _smtpMail.SendEmail(dto.personalReg.Email, messageBody, "Edurex Onboarding.");

                        await _context.SaveChangesAsync();

                        //Create acccount wallet
                        int maxWalletId = await _context.UserWallet.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();//.MaxAsync(x => x.Id) ;

                        int newWalletId = maxWalletId + 1;

                        //Currency Id will be changed to NGN id once the whole currency is available in the db
                        await _projectServices.CreateWallet(new UserWallet
                        {
                            WalletId = ZeroPadUp(newWalletId.ToString(), 5, "3500") + 0,
                            CurrencyId = 1,
                            UserId = user.Id,
                            AvailableBalance = 0.0m,
                            SavingBalance = 0.0m,
                            EscrowBalance = 0.0m,
                            CreatedDate = DateTime.Now,

                        });

                        //Populate UserReferral table
                        //---------------------------
                        if (dto.personalReg.referralCode != null && dto.personalReg.referralCode != "")
                        {
                            var referralId = await _userManager.Users.Where(x => x.ReferralCode == dto.personalReg.referralCode).Select(x => x.Id).FirstOrDefaultAsync();
                            var refe = new UserReferred
                            {
                                ReferralId = referralId,
                                ReferredUserId = user.Id
                            };

                            await _context.UserReferred.AddAsync(refe);
                            await _context.SaveChangesAsync();
                        }

                        var res = await RegisteredStudents();

                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Successful", Successful = true };
                    }


                    return new ResponseList<RegisteredStudents>() { Data = null, Message = "Not Successful", Successful = false };
                }
                else if (dto.accountType.ToLower() == "nysc")
                {
                    var usernameExist = await _context.users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    if (usernameExist != null)
                    {
                        var res = await RegisteredStudents();
                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Email exist", Successful = false };
                    }

                    var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    if (currentuser != null)
                    {
                        var res = await RegisteredStudents();
                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Phone number exist.", Successful = false };

                    }
                    var user = new Users
                    {
                        FirstName = dto.personalReg.FirstName,
                        LastName = dto.personalReg.LastName,
                        MiddleName = dto.personalReg.MiddleName,
                        CityId = dto.personalReg.CityId,
                        Email = dto.personalReg.Email.ToLower(),
                        Address = dto.personalReg.Address,
                        Gender = dto.personalReg.Gender,
                        AlternatePhone = dto.personalReg.AlternatePhone,
                        PhoneNumber = dto.personalReg.PhoneNo,
                        UserName = dto.personalReg.Username.ToLower(),
                        StudentNumber = GenerateStudentId(),
                        Status = UserStatusEnums.Active,
                       // RoleId = studentRoleId,
                        RegisteredDate = DateTime.Now,
                        NYSC = true
                    };

                    var createdUser = await _userManager.CreateAsync(user, dto.personalReg.Password);

                    if (createdUser.Succeeded)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, dto.personalReg.Password,
                                                                    true, lockoutOnFailure: false);

                        //Send Email
                        //----------
                        var builder = new StringBuilder();
                        //await _smtpMail.SendEmail(dto.personalReg.Email, "Edurex Academy registration message", "Edurex Onboarding Successful");

                        await _context.SaveChangesAsync();

                        //Create acccount wallet
                        int maxWalletId = await _context.UserWallet.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();//.MaxAsync(x => x.Id) ;

                        int newWalletId = maxWalletId + 1;

                        //Currency Id will be changed to NGN id once the whole currency is available in the db
                        await _projectServices.CreateWallet(new UserWallet
                        {
                            WalletId = ZeroPadUp(newWalletId.ToString(), 5, "3500") + 0,
                            CurrencyId = 1,
                            UserId = user.Id,
                            AvailableBalance = 0.0m,
                            SavingBalance = 0.0m,
                            EscrowBalance = 0.0m,
                            CreatedDate = DateTime.Now,

                        });

                        var AnyNYSCPromo = await _context.Promo.Where(x => x.Category == PromoCategoryEnums.NYSC && x.StartDate.Date <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date).FirstOrDefaultAsync();
                        if (AnyNYSCPromo != null)
                        {

                        }
                        var res = await RegisteredStudents();

                        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Successful", Successful = true };
                    }


                    return new ResponseList<RegisteredStudents>() { Data = null, Message = "Not Successful", Successful = false };
                }
                else
                {
                    //var usernameExist = await _context.users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    //if (usernameExist != null)
                    //{
                    //    var res = await RegisteredStudents();
                    //    return new ResponseList<RegisteredStudents>() { Data = res, Message = "Email exist", Successful = false };
                    //}

                    //var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    //if (currentuser != null)
                    //{
                    //    var res = await RegisteredStudents();
                    //    return new ResponseList<RegisteredStudents>() { Data = res, Message = "Phone number exist.", Successful = false };

                    //}

                    //var user = new Users
                    //{
                    //    FirstName = dto.businessReg.FirstName,
                    //    LastName = dto.businessReg.LastName,
                    //    MiddleName = dto.businessReg.MiddleName,
                    //    CityId = dto.businessReg.CityId,
                    //    Email = dto.businessReg.Email.ToLower(),
                    //    Address = dto.businessReg.Address,
                    //    Gender = dto.businessReg.Gender,
                    //    AlternatePhone = dto.businessReg.AlternatePhone,
                    //    PhoneNumber = dto.businessReg.PhoneNo,
                    //    UserName = dto.businessReg.Username.ToLower(),
                    //    StudentNumber = GenerateStudentId(),
                    //    Status = UserStatusEnums.Active,
                    //    RoleId = studentRoleId,
                    //    RegisteredDate = DateTime.Now
                    //};

                    //var createdUser = await _userManager.CreateAsync(user, dto.businessReg.Password);

                    //if (createdUser.Succeeded)
                    //{
                    //    var result = await _signInManager.PasswordSignInAsync(user, dto.businessReg.Password,
                    //                                                true, lockoutOnFailure: false);
                    //    var business = new Businesses
                    //    {
                    //        Address = dto.businessReg.BusinessAddress,
                    //        Email = dto.businessReg.BusinessEmail,
                    //        Name = dto.businessReg.BusinessName,
                    //        Phone = dto.businessReg.BusinessPhone,
                    //        CityId = dto.businessReg.BusinessCityId
                    //    };

                    //    await _context.Businesses.AddAsync(business);

                    //    if (await _context.SaveChangesAsync() > 0)
                    //    {
                    //        var businessUser = new BusinessesUsers
                    //        {
                    //            UserRole = dto.businessReg.UserRole,
                    //            BusinessId = business.Id,
                    //            UserId = user.Id
                    //        };

                    //        await _context.BusinessesUsers.AddAsync(businessUser);
                    //        await _context.SaveChangesAsync();

                    //        //Create acccount wallet
                    //        int maxWalletId = await _context.UserWallet.MaxAsync(x => x.Id); ;
                    //        int newWalletId = maxWalletId + 1;

                    //        //Currency Id will be changed to NGN id once the whole currency is available in the db
                    //        await _projectServices.CreateWallet(new UserWallet
                    //        {
                    //            WalletId = ZeroPadUp(newWalletId.ToString(), 5, "3500") + 0,
                    //            CurrencyId = 1,
                    //            UserId = user.Id,
                    //            AvailableBalance = 0.0m,
                    //            SavingBalance = 0.0m,
                    //            EscrowBalance = 0.0m,
                    //            CreatedDate = DateTime.Now,

                    //        });
                    //        var res = await RegisteredStudents();

                    //        return new ResponseList<RegisteredStudents>() { Data = res, Message = "Successful", Successful = true };
                    //    }
                    //return new ResponseList<RegisteredStudents>() { Data = null, Message = "Not Successful", Successful = false };

                    //}

                    return new ResponseList<RegisteredStudents>() { Data = null, Message = "Not Successful", Successful = false };
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<PaymentInitialiseResponse> Enrollment2(Register2DTO dto)
        {
            try
            {
                var paymentList = new List<PaymentDTO2>();
                var user = await _userManager.FindByIdAsync(dto.userId);
                if (user != null)
                {
                    var subRef = "";
                    PaymentMethodEnums paymentM = new PaymentMethodEnums();
                    paymentM = PaymentMethodEnums.Offline;
                    subRef = dto.UserPayment.OfflinePaymentRef;
                    
                    if (dto.userCourseOption != null && dto.userCourseOption.CoursePriceOptionId > 0)
                    {
                        UserCourseStatusEnums status = new UserCourseStatusEnums();
                        //float perAmount = 0;
                        var courseDate = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.Id == dto.userCourseOption.CoursePriceOptionId).Select(x => new { startD = x.StartDate.Date, endD = x.EndDate.Date, coursePrice = x.Amount }).FirstOrDefaultAsync();
                        if (courseDate != null)
                        {
                            if (courseDate.startD > DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.Pending;
                            }
                            else if (courseDate.startD <= DateTime.Now.Date && courseDate.endD >= DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.InProgress;
                            }
                            else if (courseDate.endD < DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.Completed;
                            }
                        }
                        var certTypeId = 0;

                        if (dto.PromoCode != null && dto.PromoCode != "")
                        {
                            var promoInfo = await _context.Promo.Where(x => x.Code == dto.PromoCode).FirstOrDefaultAsync();
                            if (promoInfo != null)
                            {
                                var promoValue = (promoInfo.PromoPercentage / 100) * courseDate.coursePrice;

                                if (dto.userCourseOption.AmountPaid <= promoValue)
                                {
                                    return new PaymentInitialiseResponse() { errorMessage = "Amount payable must be more than the promo value." };
                                }

                                //var promoUsage = new PromoUsageHistory
                                //{
                                //    PromoId = promoInfo.Id,
                                //    UserId = dto.userId
                                //};
                                //await _context.PromoUsageHistory.AddAsync(promoUsage);
                                //await _context.SaveChangesAsync();

                                var promoDTO = new PaymentDTO2()
                                {
                                    Amount = (decimal)promoValue,
                                    PaymentFor = paymentForEnums.Promo,
                                    UserPaymentForId = promoInfo.Id,
                                    UserId = dto.userId,
                                    paymentRef = subRef
                                };

                                paymentList.Add(promoDTO);
                            }


                        }
                        var newCourseOp = new UserCourses
                        {
                            CoursePriceOptionId = dto.userCourseOption.CoursePriceOptionId,
                            CertificateTypeId = certTypeId,
                            deliveryStateId = dto.userCourseOption.deliveryStateId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                            Status = status,
                            PaymentStatus = UserProgramPaymentStatusEnums.Outstanding
                        };

                        await _context.UserCourses.AddAsync(newCourseOp);
                        await _context.SaveChangesAsync();

                        int refer = 0;
                        float discount = 0;

                        if (dto.refCode != null && dto.refCode != "")
                        {

                            //Get if the person is a new user
                            //-------------------------------

                            var referral = await _context.UserReferred.Include(x => x.Referral).Where(x => x.ReferredUserId == dto.userId).FirstOrDefaultAsync();

                            if (referral != null)
                            {
                                var defaultRole = await _roleManager.FindByIdAsync(referral.Referral.DefaultRole);

                                if (defaultRole.Name == "Staff")
                                {
                                    discount = (7 * courseDate.coursePrice) / 100;
                                    // perAmount = dto.userCourseOption.AmountPaid;
                                    referral.ReferralDiscount = 3;
                                    referral.ReferredDiscount = 7;
                                }
                                else if (defaultRole.Name == "Freelance")
                                {
                                    discount = (5 * courseDate.coursePrice) / 100;
                                    //perAmount = dto.userCourseOption.AmountPaid;
                                    referral.ReferralDiscount = 5;
                                    referral.ReferredDiscount = 5;
                                }

                                refer = referral.Id;

                                var refePH = new UserReferralPaymentHistory
                                {
                                    PaymentRef = subRef,
                                    UserReferId = referral.Id,
                                    Amount = dto.userCourseOption.AmountPaid,
                                    UserCourseId = newCourseOp.Id
                                };

                                await _context.UserReferralPaymentHistory.AddAsync(refePH);
                                await _context.SaveChangesAsync();
                            }
                        }

                        var pDTO = new PaymentDTO2()
                        {
                            Amount = (decimal)dto.userCourseOption.AmountPaid,
                            PaymentFor = paymentForEnums.Course,
                            UserPaymentForId = newCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = subRef,
                            // perAmount = perAmount,
                            discountAm = discount
                        };

                        paymentList.Add(pDTO);
                        
                        //var userCoursePayment = new UserPaymentHistory
                        //{
                        //    Amount =(decimal)dto.userCourseOption.AmountPaid,
                        //    PaymentDate =DateTime.Now,
                        //    PaymentFor = paymentForEnums.Course,
                        //    PaymentMethodId = paymentM,
                        //    StatusId = PaymentStatusEnums.Initialized,
                        //    UserId = dto.userId,
                        //    UserPaymentForId = newCourseOp.Id,
                        //    a
                        //};
                    }

                    if (dto.userCertificationOption != null && dto.userCertificationOption.CertificationPriceOptionId > 0)
                    {
                        UserCourseStatusEnums status = new UserCourseStatusEnums();

                        var courseDate = await _context.CertificationPriceOptions.Include(x => x.Certification).Where(x => x.Id == dto.userCertificationOption.CertificationPriceOptionId).Select(x => new { startD = x.ExamDate.Date }).FirstOrDefaultAsync();
                        if (courseDate != null)
                        {
                            if (courseDate.startD < DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.Pending;
                            }
                            else if (courseDate.startD == DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.InProgress;
                            }
                            else if (courseDate.startD > DateTime.Now.Date)
                            {
                                status = UserCourseStatusEnums.Completed;
                            }
                        }
                        var newCourseOp = new UserCertifications
                        {
                            CertificationPriceOptionId = dto.userCertificationOption.CertificationPriceOptionId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                            Status = status
                        };

                        await _context.UserCertifications.AddAsync(newCourseOp);
                        await _context.SaveChangesAsync();
                        var rateAmount = await _projectServices.GetCertificationConvertedValuebyCertOptId(dto.userCertificationOption.CertificationPriceOptionId);
                        var amount = rateAmount.Split(',')[0];
                        var rate = rateAmount.Split(',')[1];
                        var charges = rateAmount.Split(',')[2];
                        var pDTO = new PaymentDTO2()
                        {
                            Amount = Convert.ToDecimal(amount) * Convert.ToDecimal(rate) + Convert.ToDecimal(charges),
                            PaymentFor = paymentForEnums.Certifications,
                            UserPaymentForId = newCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = subRef
                        };
                        paymentList.Add(pDTO);
                    }

                   
                }
                return new PaymentInitialiseResponse() { errorMessage = "Invalid user details." };

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<InstitutionDestOffVM> InstitutionDestOffList(int InstitutionId)
        {
            try
            {
                var result = await _context.DeskOfficers.Include(x => x.Institution).Where(x=>x.InstitutionId == InstitutionId).ToListAsync();

                var vm = new InstitutionDestOffVM
                {
                    institutionDestO = result,
                    InstitutionId = InstitutionId
                };

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<InstitutionDestOffVM> DeleteInstitutionDestOff(int Id)
        {
            var exist = await _context.DeskOfficers.Where(x => x.Id == Id).FirstOrDefaultAsync();
            if (exist != null)
            {
                _context.DeskOfficers.Remove(exist);
            }
            await _context.SaveChangesAsync();

            var result = await _context.DeskOfficers.Include(x => x.Institution).Where(x => x.InstitutionId == exist.InstitutionId).ToListAsync();

            var vm = new InstitutionDestOffVM
            {
                institutionDestO = result,
                InstitutionId = exist.InstitutionId
            };

            return vm;
        }
        public async Task<InstitutionDestOffVM> EditInstitutionDestOff(InstitutionDeskOfficerEdit dto)
        {
            var Exists = await _context.DeskOfficers.SingleOrDefaultAsync(s => s.Id == dto.Id);

            if (Exists != null)
            {
                Exists.CanLogin = dto.CanLogin;
                Exists.FirstName  = dto.FirstName;
                Exists.LastName  = dto.LastName;
                Exists.Phone = dto.Phone;
                Exists.Title = dto.Title;                

                _context.DeskOfficers.Update(Exists);

                await _context.SaveChangesAsync();
            }

            var result = await _context.DeskOfficers.Include(x => x.Institution).Where(x => x.InstitutionId == Exists.InstitutionId).ToListAsync();

            var vm = new InstitutionDestOffVM
            {
                institutionDestO = result,
                InstitutionId = Exists.InstitutionId
            };

            return vm;
        }
        private string GenerateDeskOfficerPassword()
        {
            StringBuilder builder = new StringBuilder();

            Random rstToken = new Random();

            char ch;
            for (int i = 0; i < 6; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rstToken.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
        public async Task<InstitutionDestOffVM> AddInstitutionDestOff(InstitutionDestOffAdd dto)
        {
            var roleExists = await _context.DeskOfficers.SingleOrDefaultAsync(s => s.Email == dto.Email);

            if (roleExists == null)
            {
                var password = GenerateDeskOfficerPassword();
                var inst = new DeskOfficers()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Title = dto.Title,
                    CanLogin = dto.CanLogin,
                    Email = dto.Email,
                    Phone =dto.Phone,
                    InstitutionId =dto.InstitutionId,
                    Password = password
                };
                await _context.DeskOfficers.AddAsync(inst);

                await _context.SaveChangesAsync();

                var messageBody = $"Your account on EDUREX has been created. Your username is {dto.Email} and your password is {password}";
                //Send Email
                //----------
                var builder = new StringBuilder();
                await _messagingService.SendEmail(dto.Email, messageBody, "Edurex Onboarding Successful");

            }

            var result = await _context.DeskOfficers.Include(x => x.Institution).Where(x => x.InstitutionId == dto.InstitutionId).ToListAsync();

            var vm = new InstitutionDestOffVM
            {
                institutionDestO = result,
                InstitutionId = dto.InstitutionId
            };

            return vm;
        }
        public async Task<List<InstitutionType>> InstitutionTypes()
        {
            try
            {
                var result = await _context.InstitutionType.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<InstitutionRecord>> Institutions()
        {
            try
            {
                var result = await _context.Institutions.Include(x=>x.InstitutionType).Where(x=>x.ShortName != null).Select(x=> new InstitutionRecord
                { 
                    InstitutionType =x.InstitutionType.Name,
                    Name =x.Name,
                    Id =x.Id,
                    ShortName =x.ShortName,
                    CityId=x.CityId,
                    Status =x.Status,
                    TotalOfficer =_context.DeskOfficers.Where(m=>m.InstitutionId == x.Id).Count()
                    //address= _addressServices.GetAddressByCityId(x.CityId)

                }).ToListAsync();


                if(result.Count()>0)
                {
                    foreach (var item in result)
                    {
                        if(item.CityId != null && item.CityId>0)
                        {
                            var addr = await _projectServices.GetAddressByCityId(item.CityId);
                            if (addr.Contains(","))
                            {
                                var splitAdd = addr.Split(",");
                                item.City = splitAdd[2].ToString();
                                item.State = splitAdd[1].ToString();
                                item.Country = splitAdd[0].ToString();
                            }
                        }
                       
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<InstitutionRecord>> AddInstitution(AddInstitutionDTO dto, string LogoPath)
        {
            var roleExists = await _context.Institutions.SingleOrDefaultAsync(s => s.Name == dto.Name);

            if (roleExists == null)
            {
                var inst = new Institutions()
                {
                    Name = dto.Name,
                    CityId =dto.CityId,
                    InstitutionTypeId =dto.InstitutionTypeId,
                    ShortName =dto.ShortName,
                    Status =UserStatusEnums.Active,
                    LogoPath = LogoPath
                };
                await _context.Institutions.AddAsync(inst);

                await _context.SaveChangesAsync();
            }


            var result = await _context.Institutions.Include(x => x.InstitutionType).Where(x => x.ShortName != null).Select(x => new InstitutionRecord
            {
                InstitutionType = x.InstitutionType.Name,
                Name = x.Name,
                Id = x.Id,
                ShortName = x.ShortName,
                CityId = x.CityId,
                Status = x.Status,
                TotalOfficer = _context.DeskOfficers.Where(m => m.InstitutionId == x.Id).Count()
                //address= _addressServices.GetAddressByCityId(x.CityId)

            }).ToListAsync();


            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    if (item.CityId != null && item.CityId > 0)
                    {
                        var addr = await _projectServices.GetAddressByCityId(item.CityId);
                        if (addr.Contains(","))
                        {
                            var splitAdd = addr.Split(",");
                            item.City = splitAdd[2].ToString();
                            item.State = splitAdd[1].ToString();
                            item.Country = splitAdd[0].ToString();
                        }
                    }
                }
            }
            return result;
        }
        public async Task<List<InstitutionRecord>> EditInstitution(InstitutionEditVM dto)
        {
            var Exists = await _context.Institutions.SingleOrDefaultAsync(s => s.Id == dto.InstitutionId);

            if (Exists != null)
            {
                Exists.InstitutionTypeId = dto.institutionEdit.InstitutionTypeId;
                Exists.CityId = dto.institutionEdit.CityId;
                Exists.Name = dto.institutionEdit.Name;
                Exists.ShortName = dto.institutionEdit.ShortName;

                _context.Institutions.Update(Exists);

                await _context.SaveChangesAsync();
            }


            var result = await _context.Institutions.Include(x => x.InstitutionType).Where(x => x.ShortName != null).Select(x => new InstitutionRecord
            {
                InstitutionType = x.InstitutionType.Name,
                Name = x.Name,
                Id = x.Id,
                ShortName = x.ShortName,
                CityId = x.CityId,
                Status = x.Status,
                TotalOfficer = _context.DeskOfficers.Where(m => m.InstitutionId == x.Id).Count()
                //address= _addressServices.GetAddressByCityId(x.CityId)

            }).ToListAsync();


            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    if (item.CityId != null && item.CityId > 0)
                    {
                        var addr = await _projectServices.GetAddressByCityId(item.CityId);
                        if (addr.Contains(","))
                        {
                            var splitAdd = addr.Split(",");
                            item.City = splitAdd[2].ToString();
                            item.State = splitAdd[1].ToString();
                            item.Country = splitAdd[0].ToString();
                        }
                    }
                }
            }
            return result;
        }
        public async Task<List<InstitutionRecord>> DeleteInstitution(int Id)
        {
            var exist = await _context.Institutions.Where(x => x.Id == Id).FirstOrDefaultAsync();
            if (exist != null)
            {
                _context.Institutions.Remove(exist);
            }
            await _context.SaveChangesAsync();


            var result = await _context.Institutions.Include(x => x.InstitutionType).Where(x => x.ShortName != null).Select(x => new InstitutionRecord
            {
                InstitutionType = x.InstitutionType.Name,
                Name = x.Name,
                Id = x.Id,
                ShortName = x.ShortName,
                CityId = x.CityId,
                Status = x.Status,
                TotalOfficer = _context.DeskOfficers.Where(m => m.InstitutionId == x.Id).Count()
                //address= _addressServices.GetAddressByCityId(x.CityId)

            }).ToListAsync();


            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    if (item.CityId != null && item.CityId > 0)
                    {
                        var addr = await _projectServices.GetAddressByCityId(item.CityId);
                        if (addr.Contains(","))
                        {
                            var splitAdd = addr.Split(",");
                            item.City = splitAdd[2].ToString();
                            item.State = splitAdd[1].ToString();
                            item.Country = splitAdd[0].ToString();
                        }
                    }
                }
            }
            return result;
        }
        public async Task<List<InstitutionRecord>> InstitutionStatus(int InstitutionId)
        {
            var exist = await _context.Institutions.Where(x => x.Id == InstitutionId).FirstOrDefaultAsync();
            if (exist != null)
            {
                if (exist.Status == UserStatusEnums.Active)
                {
                    exist.Status = UserStatusEnums.Inactive;
                }
                else if (exist.Status == UserStatusEnums.Inactive)
                {
                    exist.Status = UserStatusEnums.Active;
                }
                _context.Institutions.Update(exist);
            }
            await _context.SaveChangesAsync();


            var result = await _context.Institutions.Include(x => x.InstitutionType).Where(x => x.ShortName != null).Select(x => new InstitutionRecord
            {
                InstitutionType = x.InstitutionType.Name,
                Name = x.Name,
                Id = x.Id,
                ShortName = x.ShortName,
                CityId = x.CityId,
                Status = x.Status,
                TotalOfficer = _context.DeskOfficers.Where(m => m.InstitutionId == x.Id).Count()
                //address= _addressServices.GetAddressByCityId(x.CityId)

            }).ToListAsync();


            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    if (item.CityId != null && item.CityId > 0)
                    {
                        var addr = await _projectServices.GetAddressByCityId(item.CityId);
                        if (addr.Contains(","))
                        {
                            var splitAdd = addr.Split(",");
                            item.City = splitAdd[2].ToString();
                            item.State = splitAdd[1].ToString();
                            item.Country = splitAdd[0].ToString();
                        }
                    }
                }
            }
            return result;
        }
        public async Task<ReportVM> Reports(ReportDTO dto)
        {
            try
            {
                var vm = new ReportVM();
                var totalRec = new List<details>(); 
                var reportP = new List<ReportDetails>();
                var reportC = new List<ReportDetails>();
                var reportCou = new List<ReportDetails>();
                var reportCert = new List<ReportDetails>();
                if (dto.From == null && dto.To == null)
                {

                    //Program Report                    
                    var progCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x=>x.Course).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Institution.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    var progCert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x=>x.Certification).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Institution.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportP = new List<ReportDetails>();
                    if (progCourse.Count() > 0)
                    {
                        totalRec.AddRange(progCourse);
                    }
                    if (progCert.Count() > 0)
                    {
                        totalRec.AddRange(progCert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportP.Add(reportPEach);
                        }

                    }

                    //Category Report
                    var catCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    var carCert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x=>x.Certification).ThenInclude(x => x.Category).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportC = new List<ReportDetails>();
                    if (catCourse.Count() > 0)
                    {
                        totalRec.AddRange(catCourse);
                    }
                    if (carCert.Count() > 0)
                    {
                        totalRec.AddRange(carCert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportC.Add(reportPEach);
                        }

                    }

                    //Course Report
                    var Cours = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportCou = new List<ReportDetails>();
                    if (Cours.Count() > 0)
                    {
                        totalRec.AddRange(Cours);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportCou.Add(reportPEach);
                        }

                    }

                    //Certification Report
                    var Cert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).ThenInclude(x => x.Category).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportCert = new List<ReportDetails>();
                    if (Cert.Count() > 0)
                    {
                        totalRec.AddRange(Cert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportCert.Add(reportPEach);
                        }

                    }
                }
                else
                {
                    //Program Report
                   
                    var progCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x=>x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Institution.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    var progCert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Institution.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    reportP = new List<ReportDetails>();
                    totalRec = new List<details>();
                    if (progCourse.Count() > 0)
                    {
                        totalRec.AddRange(progCourse);
                    }
                    if (progCert.Count() > 0)
                    {
                        totalRec.AddRange(progCert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportP.Add(reportPEach);
                        }

                    }

                    //Category Report
                    var catCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).Where(x => x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    var carCert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).ThenInclude(x => x.Category).Where(x => x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportC = new List<ReportDetails>();
                    if (catCourse.Count() > 0)
                    {
                        totalRec.AddRange(catCourse);
                    }
                    if (carCert.Count() > 0)
                    {
                        totalRec.AddRange(carCert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportC.Add(reportPEach);
                        }

                    }

                    //Course Report
                    var Cours = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).Where(x => x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CoursePriceOption.Course.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportCou = new List<ReportDetails>();
                    if (Cours.Count() > 0)
                    {
                        totalRec.AddRange(Cours);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            //var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid = totalPaidCourse;// + totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportCou.Add(reportPEach);
                        }

                    }

                    //Certification Report
                    var Cert = await _context.UserCertifications.Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).ThenInclude(x => x.Category).Where(x => x.RegisteredDate.Date >= dto.From.Value.Date && x.RegisteredDate.Date <= dto.To.Value.Date).Select(x => new details { Name = x.CertificationPriceOption.Certification.Category.Name, UserId = x.UserId, Status = x.User.Status }).ToListAsync();
                    totalRec = new List<details>();
                    reportCert = new List<ReportDetails>();
                    if (Cert.Count() > 0)
                    {
                        totalRec.AddRange(Cert);
                    }

                    if (totalRec.Count() > 0)
                    {
                        var name = new List<string>();
                        totalRec = totalRec.Distinct().ToList();
                        foreach (var n in totalRec)
                        {
                            name.Add(n.Name);
                        }

                        name = name.Distinct().ToList();
                        foreach (var na in name)
                        {
                            //var totalPaidCourse = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaidCertifications = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Certifications && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                            var totalPaid =totalPaidCertifications;
                            var reportPEach = new ReportDetails
                            {
                                TotalActive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Active && x.Name == na).ToList().Count():n0}",
                                TotalInactive = $"{totalRec.Where(x => x.Status == UserStatusEnums.Inactive && x.Name == na).ToList().Count():n0}",
                                TotalStudent = $"{totalRec.Where(x => x.Name == na).ToList().Count():n0}",
                                Name = na,
                                TotalAmountPaid = totalPaid.ToString("N")
                            };
                            reportCert.Add(reportPEach);
                        }

                    }
                }



                vm.RPrograms = reportP;
                vm.RCategories = reportC;
                vm.RCourses = reportCou;
                vm.RCertifications = reportCert;

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<AdminRolesdto>> AdminRoles()
        {
            try
            {
                var result = await _context.Role.Select(x => new AdminRolesdto { Id = x.Id, name = x.Name }).ToListAsync(); //_context.AllUserRoles.Where(x=>x.Name != "Student").Select(x=>new AdminRolesdto { Id = x.Id, name = x.Name}).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<AdminRolesdto>> EditRole(AdminRolesdto dto)
        {
            var result = new List<AdminRolesdto>();
            var roleExists = await _context.Role.SingleOrDefaultAsync(s => s.Id == dto.Id);  //.AllUserRoles.SingleOrDefaultAsync(s => s.Id == dto.Id);

            if (roleExists != null)
            {
                roleExists.Name = dto.name;
                //roleExists.NormalizedName = dto.name.ToUpper();

                //_context.AllUserRoles.Update(roleExists);
                await _context.SaveChangesAsync();
            }
            result = await _context.Role.Select(x => new AdminRolesdto { Id = x.Id, name = x.Name }).ToListAsync();
            return result;
        }
        public async Task<List<AdminRolesdto>> AddRole(AdminRolesdto dto)
        {
            var result = new List<AdminRolesdto>();
            var roleExist = await _roleManager.RoleExistsAsync(dto.name);
            if (!roleExist)
            {
                //create the roles and seed them to the database: Question 1
               var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.name));

                await _context.SaveChangesAsync();
            }
            //var roleExists = await _context.AllUserRoles.SingleOrDefaultAsync(s => s.Name == dto.name);

            //if (roleExists == null)
            //{
            //    AllUserRoles userR = new AllUserRoles()
            //    {
            //        Name = dto.name,
            //       // NormalizedName = dto.name.ToUpper()
            //    };

            //    await _context.AllUserRoles.AddAsync(userR);

            //    await _context.SaveChangesAsync();
            //}
            result = await _context.Role.Select(x => new AdminRolesdto { Id = x.Id, name = x.Name }).ToListAsync();
            return result;
        }
        public async Task<AdminDashboardVM> RegisterAdmin(AdminRegisVM dto)
        {

            var existingUser = await _userManager.FindByEmailAsync(dto.AdminRegis.Email);


            if (existingUser != null)
            {
                return new AdminDashboardVM() { status = "Email already exist." };
            }
            else
            {
                var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.AdminRegis.Phone);
                if (currentuser != null)
                {
                    return new AdminDashboardVM() { status = "Phone number already exist." };

                }
               // var Role = await _roleManager.FindByIdAsync(dto.AdminRegis.Role);

                var newUser = new Users
                {
                    Email = dto.AdminRegis.Email.ToLower(),
                    UserName = dto.AdminRegis.Email.ToLower(),
                    PhoneNumber = dto.AdminRegis.Phone,
                    FirstName = dto.AdminRegis.FirstName,
                    LastName = dto.AdminRegis.LastName,
                    EmailConfirmed = true,
                    Status = UserStatusEnums.Active,
                    RegisteredDate = DateTime.Now.Date,
                    DefaultRole = dto.AdminRegis.Role
                };

                var createdUser = await _userManager.CreateAsync(newUser, dto.AdminRegis.Password);

                if (createdUser.Succeeded)
                {

                    var rolede = await _roleManager.FindByIdAsync(dto.AdminRegis.Role);
                    await _userManager.AddToRoleAsync(newUser, rolede.Name);

                    //newUser.DefaultRole = rolede.Id;

                    await _userManager.UpdateAsync(newUser);
                    ////Send Email
                    ////----------
                    //var builder = new StringBuilder();
                    //await _messagingService.SendEmail(dto.AdminRegis.Email.ToLower(), "Edurex Academy registration message", "Registration Successful");

                    ////Send SMS
                    ////--------
                    //await _messagingService.SendSMS(dto.AdminRegis.Phone, "Edurex Registration Successful");
                    // var role = await _context.AllUserRoles.Where(x=>x.Id == newUser.RoleId).Select(x=>x.Name).FirstOrDefaultAsync();
                    return new AdminDashboardVM() { status = "Successful", email =dto.AdminRegis.Email.ToLower(), role = rolede.Name };
                }
                return new AdminDashboardVM() { status = "Error creating user." };
            }
        }

        public async Task<DashboardVM> DashboardRe(string email)
        {

            var existingUser = await _userManager.FindByEmailAsync(email);//.Users.Include(x => x.Role).Where(x => x.Email == email).FirstOrDefaultAsync();


            if (existingUser != null)
            {               
                var vm = new DashboardVM();
                var defaultRole = await _roleManager.FindByIdAsync(existingUser.DefaultRole);
                GeneralClass.FullName = $"{existingUser.FirstName} {existingUser.LastName} - {(defaultRole.Name).ToUpper()}";

                string ipAddress = "";
               var IPs = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,address => address.AddressFamily == AddressFamily.InterNetwork);

                if (IPs.Length > 0)
                {
                    ipAddress = IPs[0].ToString();
                }

                var login = new LoginTrail
                {
                    LogTime = DateTime.Now,
                    UserId = existingUser.Id,
                    IP = ipAddress
                };

                await _context.LoginTrail.AddAsync(login);
                await _context.SaveChangesAsync();

                //Today's Record
                var studentTodayR = await (from ur in _context.UserRoles
                                           join user in _userManager.Users on ur.UserId equals user.Id
                                           join role in _context.Role on ur.RoleId equals role.Id
                                           where role.Name == "Student"
                                           && user.RegisteredDate.Date == DateTime.Now.Date
                                           select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _userManager.Users.Include(x=>x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date == DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                var studentTodayWithoutCou = await (from ur in _context.UserRoles
                                                    join user in _userManager.Users on ur.UserId equals user.Id
                                                    join role in _context.Role on ur.RoleId equals role.Id
                                                    where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                        && role.Name == "Student"
                                        && user.RegisteredDate.Date == DateTime.Now.Date
                                        select new {userId = user.Id }).Distinct().CountAsync();
                var studentTodayWithCou = await (from ucourse in _context.UserCourses
                                                 join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                 join user in _userManager.Users on urole.UserId equals user.Id
                                                 join role in _context.Role on urole.RoleId equals role.Id
                                                 where role.Name == "Student"
                                                 && user.RegisteredDate.Date == DateTime.Now.Date
                                                 select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _context.UserCourses.Include(x=>x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date == DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();
                var TodayPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                             join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                             join user in _userManager.Users on urole.UserId equals user.Id
                                             join role in _context.Role on urole.RoleId equals role.Id
                                             where uPaid.PaymentFor == paymentForEnums.Course
                                             && role.Name == "Student"
                                             && uPaid.PaymentDate.Date == DateTime.Now.Date
                                             && uPaid.StatusId == PaymentStatusEnums.Paid
                                             select uPaid.Amount).SumAsync();
                //await _context.UserPaymentHistory.Include(x=>x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                var TodayPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                     join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                     join user in _userManager.Users on urole.UserId equals user.Id
                                                     join role in _context.Role on urole.RoleId equals role.Id
                                                     where uPaid.PaymentFor == paymentForEnums.Certifications
                                                     && role.Name == "Student"
                                                     && uPaid.PaymentDate.Date == DateTime.Now.Date
                                                     && uPaid.StatusId == PaymentStatusEnums.Paid
                                                     select uPaid.Amount).SumAsync(); 
                //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                
                var studentTodayPayment = TodayPaidCourse + TodayPaidCertifications;
                var todayTotalRecord = new TotalRecord()
                {
                    AmountPaid = studentTodayPayment.ToString("N"),
                    RegisteredStudents = $"{studentTodayR:n0}",
                    WithCourses = $"{studentTodayWithCou:n0}",
                    WithoutCourses = $"{studentTodayWithoutCou:n0}"
                };

                //This Week Record
                var TodayDayWeek = DateTime.Now.DayOfWeek.ToString();
                var studentWeekR = 0;
                var studentWeekWithoutCou = 0;
                var studentWeekWithCou = 0;
                decimal studentWeekPayment = 0;
                if (TodayDayWeek.ToLower() == "sunday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date == DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date == DateTime.Now).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                        && role.Name == "Student"
                                                        && user.RegisteredDate.Date == DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date == DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date == DateTime.Now).Select(x => x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course 
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date == DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync();

                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date == DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "monday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-1) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-1) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-1) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-1) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-1) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();

                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "tuesday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-2) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-2) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                  && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-2) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-2) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-2) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "wednesday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-3) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-3) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-3) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-3) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-3) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "thursday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-4) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-4) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-4) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();

                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-4) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-4) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "friday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-5) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-5) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-5) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-5) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-5) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x=>x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                else if (TodayDayWeek.ToLower() == "saturday")
                {
                    studentWeekR = await (from ur in _context.UserRoles
                                          join user in _userManager.Users on ur.UserId equals user.Id
                                          join role in _context.Role on ur.RoleId equals role.Id
                                          where role.Name == "Student"
                                          && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-6) && user.RegisteredDate.Date <= DateTime.Now.Date
                                          select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-6) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    studentWeekWithoutCou = await (from ur in _context.UserRoles
                                                   join user in _userManager.Users on ur.UserId equals user.Id
                                                   join role in _context.Role on ur.RoleId equals role.Id
                                                   where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                   && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-6) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                   select new { userId = user.Id }).Distinct().CountAsync();
                    studentWeekWithCou = await (from ucourse in _context.UserCourses
                                                join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where role.Name == "Student"
                                                && user.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-6) && user.RegisteredDate.Date <= DateTime.Now.Date
                                                select new { userId = user.Id }).Distinct().CountAsync(); 
                    //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Date >= DateTime.Now.Date.AddDays(-6) && x.RegisteredDate.Date <= DateTime.Now.Date).Select(x => x.UserId).Distinct().CountAsync();
                    var WeekPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                                join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                join user in _userManager.Users on urole.UserId equals user.Id
                                                join role in _context.Role on urole.RoleId equals role.Id
                                                where uPaid.PaymentFor == paymentForEnums.Course
                                                && role.Name == "Student"
                                                && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                && uPaid.StatusId == PaymentStatusEnums.Paid
                                                select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var WeekPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                        join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                        join user in _userManager.Users on urole.UserId equals user.Id
                                                        join role in _context.Role on urole.RoleId equals role.Id
                                                        where uPaid.PaymentFor == paymentForEnums.Certifications
                                                        && role.Name == "Student"
                                                        && uPaid.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && uPaid.PaymentDate.Date <= DateTime.Now.Date
                                                        && uPaid.StatusId == PaymentStatusEnums.Paid
                                                        select uPaid.Amount).SumAsync(); 
                    //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    studentWeekPayment = WeekPaidCourse + WeekPaidCertifications;

                }
                var weekTotalRecord = new TotalRecord()
                {
                    AmountPaid = studentWeekPayment.ToString("N"),
                    RegisteredStudents = $"{studentWeekR:n0}",
                    WithCourses = $"{studentWeekWithCou:n0}",
                    WithoutCourses = $"{studentWeekWithoutCou:n0}"
                };
                //This Month Record
                var studentMonthR = await (from ur in _context.UserRoles
                                           join user in _userManager.Users on ur.UserId equals user.Id
                                           join role in _context.Role on ur.RoleId equals role.Id
                                           where role.Name == "Student"
                                           && user.RegisteredDate.Month == DateTime.Now.Date.Month
                                           select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.RegisteredDate.Date.Month == DateTime.Now.Date.Month).Select(x => x.Id).Distinct().CountAsync();
                var studentMonthWithoutCou = await (from ur in _context.UserRoles
                                                    join user in _userManager.Users on ur.UserId equals user.Id
                                                    join role in _context.Role on ur.RoleId equals role.Id
                                                    where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                   && role.Name == "Student"
                                                    && user.RegisteredDate.Month == DateTime.Now.Month
                                                    select new { userId = user.Id }).Distinct().CountAsync();
                var studentMonthWithCou = await (from ucourse in _context.UserCourses
                                                 join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                                 join user in _userManager.Users on urole.UserId equals user.Id
                                                 join role in _context.Role on urole.RoleId equals role.Id
                                                 where role.Name == "Student"
                                                 && user.RegisteredDate.Month == DateTime.Now.Date.Month
                                                 select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student" && x.RegisteredDate.Month == DateTime.Now.Month).Select(x => x.UserId).Distinct().CountAsync();
                var MonthPaidCourse = await (from uPaid in _context.UserPaymentHistory
                                             join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                             join user in _userManager.Users on urole.UserId equals user.Id
                                             join role in _context.Role on urole.RoleId equals role.Id
                                             where uPaid.PaymentFor == paymentForEnums.Course
                                             && role.Name == "Student"
                                             && uPaid.PaymentDate.Month == DateTime.Now.Month
                                             && uPaid.StatusId == PaymentStatusEnums.Paid
                                             select uPaid.Amount).SumAsync(); 
                //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Month && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                var MonthPaidCertifications = await (from uPaid in _context.UserPaymentHistory
                                                     join urole in _context.UserRole on uPaid.UserId equals urole.UserId
                                                     join user in _userManager.Users on urole.UserId equals user.Id
                                                     join role in _context.Role on urole.RoleId equals role.Id
                                                     where uPaid.PaymentFor == paymentForEnums.Certifications
                                                     && role.Name == "Student"
                                                     && uPaid.PaymentDate.Month == DateTime.Now.Month
                                                     && uPaid.StatusId == PaymentStatusEnums.Paid
                                                     select uPaid.Amount).SumAsync(); 
                //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Month && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                var studentMonthPayment = MonthPaidCourse + MonthPaidCertifications;

                var monthTotalRecord = new TotalRecord()
                {
                    AmountPaid = studentMonthPayment.ToString("N"),
                    RegisteredStudents = $"{studentMonthR:n0}",
                    WithCourses = $"{studentMonthWithCou:n0}",
                    WithoutCourses = $"{studentMonthWithoutCou:n0}"
                };

                var totP = await _context.Programs.CountAsync();
                var totPC = await _context.ProgramCategory.CountAsync();
                var totCou = await _context.Courses.CountAsync();
                var totCert = await _context.Certifications.CountAsync();

                //Student Block
                var studentR = await (from ur in _context.UserRoles
                                      join user in _userManager.Users on ur.UserId equals user.Id
                                      join role in _context.Role on ur.RoleId equals role.Id
                                      where role.Name == "Student"
                                      select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student").Select(x => x.Id).Distinct().CountAsync();
                var studentWithoutCou = await (from ur in _context.UserRoles
                                               join user in _userManager.Users on ur.UserId equals user.Id
                                               join role in _context.Role on ur.RoleId equals role.Id
                                               where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                    && role.Name == "Student"
                                                    select new { userId = user.Id }).Distinct().CountAsync();
                var studentWithCou = await (from ucourse in _context.UserCourses
                                            join urole in _context.UserRole on ucourse.UserId equals urole.UserId
                                            join user in _userManager.Users on urole.UserId equals user.Id
                                            join role in _context.Role on urole.RoleId equals role.Id
                                            where role.Name == "Student"
                                            select new { userId = user.Id }).Distinct().CountAsync();
                //await _context.UserCourses.Include(x => x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student").Select(x => x.UserId).Distinct().CountAsync();
                var studentBlocked = await (from ur in _context.UserRoles
                                            join user in _userManager.Users on ur.UserId equals user.Id
                                            join role in _context.Role on ur.RoleId equals role.Id
                                            where role.Name == "Student"
                                            && user.Status == UserStatusEnums.Blocked
                                            select new { userId = user.Id }).Distinct().CountAsync(); 
                //await _userManager.Users.Include (x=>x.Role).Where(x => x.Role.Name == "Student" && x.Status == UserStatusEnums.Blocked).Select(x => x.Id).Distinct().CountAsync();
                var studentRecord = new TotalStudentRecord
                {
                    BlockedStudent = $"{studentBlocked:n0}",
                    RegisteredStudents = $"{studentR:n0}",
                    WithoutCourses = $"{studentWithoutCou:n0}",
                    WithCourses = $"{studentWithCou:n0}"
                };

                //Latest Log
                var logR = await _context.LoginTrail.Include(x=>x.User).OrderByDescending(x => x.Id).Select(x => new RecentLoginRecord { email = x.User.Email, fullName =x.User.FirstName + " " + x.User.LastName, log = x.LogTime, Ip=x.IP}).Take(10).ToListAsync();

                vm.today = todayTotalRecord;
                vm.thisWeek = weekTotalRecord;
                vm.thisMonth = monthTotalRecord;
                vm.totalPrograms = $"{totP:n0}";
                vm.totalCategories = $"{totPC:n0}" ;
                vm.totalCertifications = $"{totCert:n0}";
                vm.totalCourses = $"{totCou:n0}";
                vm.studentBlock = studentRecord;
                vm.logRecord = logR;

                return vm;
            }
            else
            {
                return null;
            }
        }
        public async Task<List<object>> chartUserbyRole()
        {
            var chartData = new List<object>();
            chartData.Add(new object[]
                    {
                            "Name", "Total"
                    });
            var dbRecord = await (from ur in _context.UserRoles
                                  join u in _userManager.Users on ur.UserId equals u.Id
                                  join r in _context.Role on ur.RoleId equals r.Id
                                  group u by r.Name into g
                                  select new ChartRecord { Value = g.Key, Total = g.Count() }).ToListAsync();

            //await (from u in _userManager.Users 
            //                    join r in _context.AllUserRoles on u.RoleId equals r.Id
            //                    where r.Name != "Student"
            //                    group u by r.Name into g
            //                    select new ChartRecord { Value = g.Key, Total = g.Count() }).ToListAsync();

            //Add db record to chart
            foreach(var item in dbRecord)
            {
                chartData.Add(new object[]
                {
                   item.Value, item.Total
                });
            }

            return chartData;
        }
        public async Task<List<object>> chartEnrollmentbyQueryString(string param)
        {
            var chartData = new List<object>();
            chartData.Add(new object[]
            {
               "Name", "Total"
            });
            if (param == "Programs")
            {
                var CoursebyProgram = await (from uc in _context.UserCourses
                                       join u in _userManager.Users on uc.UserId equals u.Id
                                       join co in _context.CoursePriceOptions on uc.CoursePriceOptionId equals co.Id
                                       join c in _context.Courses on co.CourseId equals c.Id
                                      join ca in _context.ProgramCategory on c.CategoryId equals ca.Id
                                      join p in _context.Institutions on ca.InstitutionId equals p.Id
                                      select new EnrollmentGroupByDTO { userId = u.Id, Name = p.Name}).Distinct().ToListAsync();

                var CertbyProgram = await (from uc in _context.UserCertifications
                                           join u in _userManager.Users on uc.UserId equals u.Id
                                           join co in _context.CertificationPriceOptions on uc.CertificationPriceOptionId equals co.Id
                                           join c in _context.Certifications on co.CertificationId equals c.Id
                                           join ca in _context.ProgramCategory on c.CategoryId equals ca.Id
                                           join p in _context.Institutions on ca.InstitutionId equals p.Id
                                           select new EnrollmentGroupByDTO { userId = u.Id, Name = p.Name }).Distinct().ToListAsync();

                var byProgram = new List<EnrollmentGroupByDTO>();
                if(CoursebyProgram.Count()>0)
                {
                    byProgram.AddRange(CoursebyProgram);
                }

                if (CertbyProgram.Count() > 0)
                {
                    byProgram.AddRange(CertbyProgram);
                }

                byProgram = byProgram.Distinct().ToList();

                var chartR = new List<ChartRecord>();

                chartR= byProgram.GroupBy(p => p.Name, p => p.userId,(key, g) => new ChartRecord { Value = key, Total = g.Count() }).ToList();

                //                       group u by p.Name into g
                //select new ChartRecord { Value = g.Key, Total = g.Count() }).Distinct().ToListAsync();

                //Add db record to chart
                if (chartR.Count()>0)
                {
                    foreach (var item in chartR)
                    {
                        chartData.Add(new object[]
                        {
                            item.Value, item.Total
                        });
                    }
                }
                
            }
            else if (param == "Categories")
            {
                var CoursebyCategory = await (from uc in _context.UserCourses
                                             join u in _userManager.Users on uc.UserId equals u.Id
                                             join co in _context.CoursePriceOptions on uc.CoursePriceOptionId equals co.Id
                                             join c in _context.Courses on co.CourseId equals c.Id
                                             join ca in _context.ProgramCategory on c.CategoryId equals ca.Id
                                             select new EnrollmentGroupByDTO { userId = u.Id, Name = ca.Name }).Distinct().ToListAsync();

                var CertbyCategory = await (from uc in _context.UserCertifications
                                           join u in _userManager.Users on uc.UserId equals u.Id
                                           join co in _context.CertificationPriceOptions on uc.CertificationPriceOptionId equals co.Id
                                           join c in _context.Certifications on co.CertificationId equals c.Id
                                           join ca in _context.ProgramCategory on c.CategoryId equals ca.Id
                                           select new EnrollmentGroupByDTO { userId = u.Id, Name = ca.Name }).Distinct().ToListAsync();

                var byCategory = new List<EnrollmentGroupByDTO>();
                if (CoursebyCategory.Count() > 0)
                {
                    byCategory.AddRange(CoursebyCategory);
                }

                if (CertbyCategory.Count() > 0)
                {
                    byCategory.AddRange(CertbyCategory);
                }

                byCategory = byCategory.Distinct().ToList();

                var chartR = new List<ChartRecord>();

                chartR = byCategory.GroupBy(p => p.Name, p => p.userId, (key, g) => new ChartRecord { Value = key, Total = g.Count() }).ToList();
                if (chartR.Count() > 0)
                {
                    foreach (var item in chartR)
                    {
                        chartData.Add(new object[]
                        {
                            item.Value, item.Total
                        });
                    }
                }

            }
            else if (param == "Courses")
            {
                var byCourse = await (from uc in _context.UserCourses
                                   join u in _userManager.Users on uc.UserId equals u.Id
                                   join co in _context.CoursePriceOptions on uc.CoursePriceOptionId equals co.Id
                                   join c in _context.Courses on co.CourseId equals c.Id
                                   group u by c.Name into g
                                   select new ChartRecord { Value = g.Key, Total = g.Count() }).Distinct().ToListAsync();

                //Add db record to chart
                if (byCourse.Count() > 0)
                {
                    byCourse = byCourse.Distinct().ToList();

                    foreach (var item in byCourse)
                    {
                        chartData.Add(new object[]
                        {
                            item.Value, item.Total
                        });
                    }
                }
               
            }
            else if (param == "Certifications")
            {
                var byCertification = await (from uc in _context.UserCertifications
                                      join u in _userManager.Users on uc.UserId equals u.Id
                                      join co in _context.CertificationPriceOptions on uc.CertificationPriceOptionId equals co.Id
                                      join c in _context.Certifications on co.CertificationId equals c.Id
                                      group u by c.Name into g
                                      select new ChartRecord { Value = g.Key, Total = g.Count() }).Distinct().ToListAsync();

                //Add db record to chart
                if (byCertification.Count() > 0)
                {
                    byCertification = byCertification.Distinct().ToList();

                    foreach (var item in byCertification)
                    {
                        chartData.Add(new object[]
                        {
                            item.Value, item.Total
                        });
                    }
                }
                
            }

            return chartData;
        }
        public async Task<List<RegisteredUsers>> RegisteredUsers()
        {
            try
            {
                var enrollmentList = await (from ur in _context.UserRoles
                                    join u in _userManager.Users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name != "Student" 
                                    select new RegisteredUsers
                                    {
                                        FirstName = u.FirstName,
                                        LastName = u.LastName,
                                        RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                        Status = u.Status,
                                        Role = r.Name,
                                        Email = u.Email,
                                        LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                    }
                                        ).ToListAsync();
                //var enrollmentList = new List<RegisteredUsers>();
                //if (result.Count() > 0)
                //{                    
                //    foreach (var item in result)
                //    {
                //        var enrollment = new RegisteredUsers
                //        {
                //            FirstName = item.FirstName,
                //            LastName = item.LastName,
                //            RegisteredDate = item.RegisteredDate,
                //            Status = item.Status,
                //            Role = item.Role,
                //            Email = item.Email,
                //            LastLogin =item.LastLogin// _context.LoginTrail.Where(x=>x.UserId == item.Id).OrderByDescending(x=>x.Id).Select(x=>x.LogTime).FirstOrDefault()

                //        };

                //        enrollmentList.Add(enrollment);
                //    }

                //}
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredUsers>> EditUser(EditUser dto)
        {
            try
            {
                var user = await _userManager.Users.Where(x => x.Email == dto.Email).FirstOrDefaultAsync();
                var enrollmentList = new List<RegisteredUsers>();

                if (user != null)
                {
                    user.FirstName = dto.FirstName ?? user.FirstName;
                    user.LastName = dto.LastName ?? user.LastName;
                    user.MiddleName = dto.MiddleName ?? user.MiddleName;
                    user.Address = dto.Address ?? user.Address;
                    user.AlternatePhone = dto.AlternatePhone ?? user.AlternatePhone;

                    _context.users.Update(user);
                    await _context.SaveChangesAsync();

                     enrollmentList = await (from ur in _context.UserRoles
                                        join u in _userManager.Users on ur.UserId equals u.Id
                                        join r in _context.Role on ur.RoleId equals r.Id
                                        where r.Name != "Student"
                                        select new RegisteredUsers
                                        {
                                            FirstName = u.FirstName,
                                            LastName = u.LastName,
                                            RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                            Status = u.Status,
                                            Role = r.Name,
                                            Email = u.Email,
                                            LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                        }
                                        ).ToListAsync();
                    //if (result.Count() > 0)
                    //{
                    //    foreach (var item in result)
                    //    {
                    //        var enrollment = new RegisteredUsers
                    //        {
                    //            FirstName = item.FirstName,
                    //            LastName = item.LastName,
                    //            RegisteredDate = item.RegisteredDate,
                    //            Status = item.Status,
                    //            Role = item.Role,
                    //            Email = item.Email,
                    //            LastLogin =item.LastLogin// _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                    //        };

                    //        enrollmentList.Add(enrollment);
                    //    }

                    //}

                }
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredUsers>, Users, string>> BlockUser(BlockUserDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredUsers>();

                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if(adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        user.Status = UserStatusEnums.Blocked;

                        _context.users.Update(user);
                        await _context.SaveChangesAsync();

                        enrollmentList = await (from ur in _context.UserRoles
                                                join u in _userManager.Users on ur.UserId equals u.Id
                                                join r in _context.Role on ur.RoleId equals r.Id
                                                where r.Name != "Student"
                                                select new RegisteredUsers
                                                {
                                                    FirstName = u.FirstName,
                                                    LastName = u.LastName,
                                                    RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                                    Status = u.Status,
                                                    Role = r.Name,
                                                    Email = u.Email,
                                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                                }).ToListAsync();

                        return new Tuple<List<RegisteredUsers>,Users, string>(enrollmentList,user, "Successful");
                    }
                    return new Tuple<List<RegisteredUsers>, Users, string>(null,null, "Not Found");
                }
                
                return new Tuple<List<RegisteredUsers>, Users, string>(null,null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredUsers>, Users, string>> UnBlockUser(BlockUserDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredUsers>();
                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if (adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();


                    if (user != null)
                    {
                        user.Status = UserStatusEnums.Active;

                        _context.users.Update(user);
                        await _context.SaveChangesAsync();

                        enrollmentList = await (from ur in _context.UserRoles
                                                join u in _userManager.Users on ur.UserId equals u.Id
                                                join r in _context.Role on ur.RoleId equals r.Id
                                                where r.Name != "Student"
                                                select new RegisteredUsers
                                                {
                                                    FirstName = u.FirstName,
                                                    LastName = u.LastName,
                                                    RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                                    Status = u.Status,
                                                    Role = r.Name,
                                                    Email = u.Email,
                                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                                }
                                        ).ToListAsync();
                        return new Tuple<List<RegisteredUsers>, Users, string>(enrollmentList, user, "Successful");
                    }
                    return new Tuple<List<RegisteredUsers>, Users, string>(null, null, "Not Found");
                }

                return new Tuple<List<RegisteredUsers>, Users, string>(null, null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredUsers>> BlockedUsers()
        {
            try
            {
               var enrollmentList = await (from ur in _context.UserRoles
                                        join u in _userManager.Users on ur.UserId equals u.Id
                                        join r in _context.Role on ur.RoleId equals r.Id
                                        where r.Name != "Student"
                                        select new RegisteredUsers
                                        {
                                            FirstName = u.FirstName,
                                            LastName = u.LastName,
                                            RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                            Status = u.Status,
                                            Role = r.Name,
                                            Email = u.Email,
                                            LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                        }
                                         ).ToListAsync();
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredUsers>, Users, string>> ResetUserPassword(ResetUserPasswordDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredUsers>();

                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if (adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, dto.newPassword);
                        var res = await _userManager.UpdateAsync(user);

                        enrollmentList = await (from ur in _context.UserRoles
                                                join u in _userManager.Users on ur.UserId equals u.Id
                                                join r in _context.Role on ur.RoleId equals r.Id
                                                where r.Name != "Student"
                                                select new RegisteredUsers
                                                {
                                                    FirstName = u.FirstName,
                                                    LastName = u.LastName,
                                                    RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                                    Status = u.Status,
                                                    Role = r.Name,
                                                    Email = u.Email,
                                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                                }
                                        ).ToListAsync();
                        return new Tuple<List<RegisteredUsers>, Users, string>(enrollmentList, user, "Successful");
                    }
                    return new Tuple<List<RegisteredUsers>, Users, string>(null, null, "Not Found");
                }

                return new Tuple<List<RegisteredUsers>, Users, string>(null, null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GeneratePassword()
        {
            StringBuilder builder = new StringBuilder();

            Random rstToken = new Random();

            char ch;
            for (int i = 0; i < 8; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rstToken.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
        public async Task<Tuple<List<RegisteredUsers>, Users, string>> ResetPassword(string email)
        {
            try
            {
                var enrollmentList = new List<RegisteredUsers>();

                var user = await _userManager.Users.Where(x => x.Email == email).FirstOrDefaultAsync();

                if (user != null)
                {
                    var password = GeneratePassword();

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, password);
                    var res = await _userManager.UpdateAsync(user);

                    //Send Email
                    //----------
                    var builder = new StringBuilder();
                    await _messagingService.SendEmail(email, $"Your new password is {password}", "Edurex Password Reset");
                    // string role = "Student";
                    enrollmentList = await (from ur in _context.UserRoles
                                            join u in _userManager.Users on ur.UserId equals u.Id
                                            join r in _context.Role on ur.RoleId equals r.Id
                                            where r.Name != "Student"
                                            select new RegisteredUsers
                                            {
                                                FirstName = u.FirstName,
                                                LastName = u.LastName,
                                                RegisteredDate = u.RegisteredDate.ToString("dd/MM/yyyy"),
                                                Status = u.Status,
                                                Role = r.Name,
                                                Email = u.Email,
                                                LastLogin = _context.LoginTrail.Where(x => x.UserId == u.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                            }
                                        ).ToListAsync();
                    return new Tuple<List<RegisteredUsers>, Users, string>(enrollmentList, user, "Successful");
                }
                return new Tuple<List<RegisteredUsers>, Users, string>(null, null, "Not Found");
            
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredStudents>> RegisteredStudents()
        {
            try
            {
                var result = await _userManager.GetUsersInRoleAsync("Student"); //await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student").ToListAsync();
                var enrollmentList = new List<RegisteredStudents>();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        var enrollment = new RegisteredStudents
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            RegisteredDate = item.RegisteredDate.ToString("dd/MM/yyyy"),
                            Status = item.Status,
                            StudentNumber =item.StudentNumber,
                            Email = item.Email,
                            Phone =item.PhoneNumber,
                            TotalCourses = _context.UserCourses.Where(x=>x.UserId == item.Id).Count(),
                            TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.Id).Count(),
                            LastLogin = _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                        };

                        enrollmentList.Add(enrollment);
                    }

                }
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredStudents>> BlockedStudents()
        {
            try
            {
                var result = await _userManager.GetUsersInRoleAsync("Student");//await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student" && x.Status == UserStatusEnums.Blocked).ToListAsync();
                var enrollmentList = new List<RegisteredStudents>();
                if (result.Count() > 0)
                {
                    result = result.Where(x => x.Status == UserStatusEnums.Blocked).ToList();
                    foreach (var item in result)
                    {
                        var enrollment = new RegisteredStudents
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            RegisteredDate = item.RegisteredDate.ToString("dd/MM/yyyy"),
                            Status = item.Status,
                            StudentNumber = item.StudentNumber,
                            Email = item.Email,
                            Phone = item.PhoneNumber,
                            TotalCourses = _context.UserCourses.Where(x => x.UserId == item.Id).Count(),
                            TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.Id).Count(),
                            LastLogin = _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                        };

                        enrollmentList.Add(enrollment);
                    }

                }
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredStudents>, Users, string>> BlockStudent(BlockUserDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredStudents>();
                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if (adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();
                    

                    if (user != null)
                    {
                        user.Status = UserStatusEnums.Blocked;

                        _context.users.Update(user);
                        await _context.SaveChangesAsync();

                        var result = await _userManager.GetUsersInRoleAsync("Student");// await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student").ToListAsync();
                        if (result.Count() > 0)
                        {
                            foreach (var item in result)
                            {
                                var enrollment = new RegisteredStudents
                                {
                                    FirstName = item.FirstName,
                                    LastName = item.LastName,
                                    RegisteredDate = item.RegisteredDate.ToString("dd/MM/yyyy"),
                                    Status = item.Status,
                                    StudentNumber = item.StudentNumber,
                                    Email = item.Email,
                                    Phone = item.PhoneNumber,
                                    TotalCourses = _context.UserCourses.Where(x => x.UserId == item.Id).Count(),
                                    TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.Id).Count(),
                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                };

                                enrollmentList.Add(enrollment);
                            }

                        }
                        return new Tuple<List<RegisteredStudents>, Users, string>(enrollmentList, user, "Successful");
                    }
                    return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Not Found");
                }

                return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredStudents>, Users, string>> UnBlockStudent(BlockUserDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredStudents>();
                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if (adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();


                    if (user != null)
                    {
                        user.Status = UserStatusEnums.Active;

                        _context.users.Update(user);
                        await _context.SaveChangesAsync();

                        var result = await _userManager.GetUsersInRoleAsync("Student");// await _userManager.Users.Include(x => x.Role).Where(x => x.Role.Name == "Student").ToListAsync();
                        if (result.Count() > 0)
                        {
                            foreach (var item in result)
                            {
                                var enrollment = new RegisteredStudents
                                {
                                    FirstName = item.FirstName,
                                    LastName = item.LastName,
                                    RegisteredDate = item.RegisteredDate.ToString("dd/MM/yyyy"),
                                    Status = item.Status,
                                    StudentNumber = item.StudentNumber,
                                    Email = item.Email,
                                    Phone = item.PhoneNumber,
                                    TotalCourses = _context.UserCourses.Where(x => x.UserId == item.Id).Count(),
                                    TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.Id).Count(),
                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                };

                                enrollmentList.Add(enrollment);
                            }

                        }
                        return new Tuple<List<RegisteredStudents>, Users, string>(enrollmentList, user, "Successful");
                    }
                    return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Not Found");
                }

                return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tuple<List<RegisteredStudents>, Users, string>> ResetStudentPassword(ResetUserPasswordDTO dto)
        {
            try
            {
                var enrollmentList = new List<RegisteredStudents>();

                var adminCheck = await _context.BackOfficePIN.Include(x => x.User).Where(x => x.User.Email == dto.adminEmail && x.PIN == dto.PIN).FirstOrDefaultAsync();
                if (adminCheck != null)
                {
                    var user = await _userManager.Users.Where(x => x.Email == dto.userEmail).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, dto.newPassword);
                        var res = await _userManager.UpdateAsync(user);

                        var result = await _userManager.GetUsersInRoleAsync("Student");// await _userManager..Users.Include(x => x.Role).Where(x => x.Role.Name == "Student").ToListAsync();
                        if (result.Count() > 0)
                        {
                            foreach (var item in result)
                            {
                                var enrollment = new RegisteredStudents
                                {
                                    FirstName = item.FirstName,
                                    LastName = item.LastName,
                                    RegisteredDate = item.RegisteredDate.ToString("dd/MM/yyyy"),
                                    Status = item.Status,
                                    StudentNumber = item.StudentNumber,
                                    Email = item.Email,
                                    Phone = item.PhoneNumber,
                                    TotalCourses = _context.UserCourses.Where(x => x.UserId == item.Id).Count(),
                                    TotalCertifications = _context.UserCertifications.Where(x => x.UserId == item.Id).Count(),
                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == item.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                };

                                enrollmentList.Add(enrollment);
                            }

                        }
                        return new Tuple<List<RegisteredStudents>, Users, string>(enrollmentList, user, "Successful");
                    }
                    return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Not Found");
                }

                return new Tuple<List<RegisteredStudents>, Users, string>(null, null, "Invalid PIN");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<StudentDetails> SingleStudent(string email)
        {
            try
            {
                var result = await _userManager.Users.Where(x=>x.Email == email).FirstOrDefaultAsync();
                var enrollmentList = new StudentDetails();

                if (result != null)
                {
                    enrollmentList = new StudentDetails
                    {
                        StudentNumber = result.StudentNumber,
                        FirstName = result.FirstName,
                        LastName = result.LastName,
                        RegisteredDate = result.RegisteredDate.ToString("dd/MM/yyyy"),
                        Status = result.Status,                        
                        StCourses = _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x=>x.Course).ThenInclude(x=>x.Category).ThenInclude(x=>x.Institution).Where(x => x.UserId == result.Id).Select(x => new StudentCourses 
                                    { 
                                        CourseId = x.CoursePriceOption.CourseId, 
                                        Program=x.CoursePriceOption.Course.Category.Institution.ShortName, 
                                        Category= x.CoursePriceOption.Course.Category.Name, 
                                        CourseName = x.CoursePriceOption.Course.Name, 
                                        CertificateIssued = x.CertificateIssued, 
                                        CertificateIssuedDate = x.CertificateIssuedDate.Value.ToString("dd/MM/yyyy"), 
                                        CompletedDate = x.CompletedDate.Value.ToString("dd/MM/yyyy"), 
                                        CourseStartStatus = x.Status,
                                        RegisteredDate = x.RegisteredDate.ToString("dd/MM/yyyy"), 
                                        OptionName =x.CoursePriceOption.Name, 
                                        PaymentStatus =x.PaymentStatus, 
                                        Price =x.CoursePriceOption.Amount.ToString("N"), 
                                        AmountPaid = (_context.UserPaymentHistory.Where(c=>c.PaymentFor == paymentForEnums.Course && c.UserPaymentForId == x.Id ).Select(c=>c.Amount).Sum()).ToString("N") 
                                    }).ToList(),

                        StCerts= _context.UserCertifications.Include(x=>x.CertificationPriceOption).ThenInclude(x=>x.Currency).Include(x => x.CertificationPriceOption).ThenInclude(x=>x.Certification).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.UserId == result.Id).Select(x => new StudentCertifications 
                                    { 
                                        CertificationId = x.CertificationPriceOption.CertificationId, 
                                        Program = x.CertificationPriceOption.Certification.Category.Institution.ShortName,
                                        Category = x.CertificationPriceOption.Certification.Category.Name, 
                                        Name = x.CertificationPriceOption.Certification.Name, 
                                        CertificateIssued = x.CertificateIssued,
                                        CertificateIssuedDate = x.CertificateIssuedDate.Value.ToString("dd/MM/yyyy"), 
                                        CompletedDate = x.CompletedDate.Value.ToString("dd/MM/yyyy"),
                                        CourseStartStatus = x.Status, 
                                        RegisteredDate = x.RegisteredDate.ToString("dd/MM/yyyy"),
                                        Mode =x.CertificationPriceOption.Certification.Mode, 
                                        OrganisationName =x.CertificationPriceOption.Certification.OrganisationName, 
                                        ShortCode =x.CertificationPriceOption.Certification.ShortCode ,
                                        ExamDate  = x.CertificationPriceOption.ExamDate.ToString("dd/MM/yyyy"),
                                        PaymentStatus = x.PaymentStatus,
                                        Price = x.CertificationPriceOption.Amount.ToString(),
                                        Currency =x.CertificationPriceOption.Currency.major_symbol,
                                        AmountPaid = (_context.UserPaymentHistory.Where(c => c.PaymentFor == paymentForEnums.Certifications && c.UserPaymentForId == x.Id).Select(c => c.Amount).Sum()).ToString("N")

                                    }).ToList()

                    };

                    if (enrollmentList.StCerts.Count() > 0)
                    {
                        foreach (var item in enrollmentList.StCerts)
                        {
                            if (item.Currency == "₦")
                            {
                                item.Price = item.Currency +" " + Convert.ToDouble(item.Price).ToString("N");
                            }
                            else
                            {
                                item.Price = item.Currency + " " + item.Price;
                            }
                        }
                    }

                }
                return enrollmentList;
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                throw ex;
            }
        }
        public async Task<string> Logout()
        {

            await _signInManager.SignOutAsync();
            return "successful";
        }
        public async Task<Tuple<Users, string>> Login(LoginDTO dto)
        {

            var existingUser = await _userManager.FindByEmailAsync(dto.email);


            if (existingUser != null && await _userManager.CheckPasswordAsync(existingUser, dto.password))
            {
                var userDefaultRole = await _roleManager.FindByIdAsync(existingUser.DefaultRole);

                //var userRoles = await _userManager.GetRolesAsync(existingUser);

                //if(!userRoles.Contains(existingUser.DefaultRole))

                if (userDefaultRole.Name.ToLower() != Enum.GetName(typeof(UserRolesEnums), UserRolesEnums.Admin).ToLower() && existingUser.StaffDep == StaffDepEnums.None)
                {
                    return new Tuple<Users, string>(null, "You don't have access to this.");
                }
                var result = await _signInManager.PasswordSignInAsync(existingUser, dto.password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //var rolde = await _context.AllUserRoles.Where(x => x.Id == existingUser.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
                    if (existingUser.Status != UserStatusEnums.Blocked)
                    {
                        GeneralClass.role = userDefaultRole.Name;
                        return new Tuple<Users, string>(existingUser, "Successful");
                    }

                    return new Tuple<Users, string>(existingUser, "User Blocked");

                }
                return new Tuple<Users, string>(existingUser, "Incorrect details.");

                //var roles = await _userManager.GetRolesAsync(existingUser);
                //foreach (var role in roles)
                //{
                //    rolde = role;
                //}

                //if (existingUser.Status != UserStatusEnums.Blocked)
                //{
                //    GeneralClass.role = rolde;
                //    return new Tuple<Users, string>(existingUser, "Successful");
                //}
                //return "User Blocked";

                //return "Error";// Enum.GetName(typeof(UserRolesEnums), existingUser.Role) ;
               
            }
            else
            {
                return new Tuple<Users, string>(null, "Wrong password");
            }
        }
        public async Task<List<RegisteredStudents>> Enrollment()
        {
            try
            {
                var enrollmentList = new List<RegisteredStudents>();

                var studentUsers = await _userManager.GetUsersInRoleAsync("Student");
                if(studentUsers.Count>0)
                {
                    foreach (var item in studentUsers)
                    {
                        var result = await _context.UserCourses.Select(x => new { studentId = x.UserId }).ToListAsync();
                        if (result.Count() > 0)
                        {
                            var query = from r in result
                                        group r by r.studentId into g
                                        select new { Count = g.Count(), Value = g.Key };

                            foreach (var v in query)
                            {
                                var user = await _userManager.FindByIdAsync(v.Value);

                                var enrollment = new RegisteredStudents
                                {
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    RegisteredDate = user.RegisteredDate.ToString("dd/MM/yyyy"),
                                    Status = user.Status,
                                    StudentNumber = user.StudentNumber,
                                    Email = user.Email,
                                    TotalCourses = v.Count,
                                    Phone = user.PhoneNumber,
                                    TotalCertifications = _context.UserCertifications.Where(x => x.UserId == v.Value).Count(),
                                    LastLogin = _context.LoginTrail.Where(x => x.UserId == v.Value).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                };

                                enrollmentList.Add(enrollment);
                            }

                        }
                    }
                }
                
                //else
                //{
                //    var result2 = await _context.UserCertifications.Include(x=>x.User).ThenInclude(x => x.Role).Where(x => x.User.Role.Name == "Student").Select(x => new { studentId = x.UserId }).ToListAsync();
                
                //    if (result2.Count() > 0)
                //    {
                //        var query2 = from r in result2
                //                    group r by r.studentId into g
                //                    select new { Count = g.Count(), Value = g.Key };

                //        foreach (var m in query2)
                //        {
                //            var user2 = await _userManager.FindByIdAsync(m.Value);

                //            var enrollment = new RegisteredStudents
                //            {
                //                FirstName = user2.FirstName,
                //                LastName = user2.LastName,
                //                RegisteredDate = user2.RegisteredDate.ToString("dd/MM/yyyy"),
                //                Status = user2.Status,
                //                StudentNumber = user2.StudentNumber,
                //                Email = user2.Email,
                //                TotalCourses = 0,
                //                TotalCertifications = m.Count,
                //                LastLogin = _context.LoginTrail.Where(x => x.UserId == m.Value).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                //            };

                //            enrollmentList.Add(enrollment);
                //        }
                //        foreach (var item in result)
                //        {

                //        }

                //    }
                //}
                return enrollmentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<RegisteredStudents>> PartialEnrollment()
        {
            try
            {
                var studentUsers = await _userManager.GetUsersInRoleAsync("Student");
                var enrollmentList = new List<RegisteredStudents>();

                if (studentUsers.Count > 0)
                {
                    foreach (var item in studentUsers)
                    {
                        enrollmentList = await (from ur in _context.UserRoles
                                                    join user in _userManager.Users on ur.UserId equals user.Id
                                                    join role in _context.Role on ur.RoleId equals role.Id
                                                    where !_context.UserCourses.Any(f => f.UserId == user.Id)
                                                    //&& !_context.UserCertifications.Any(k => k.UserId == user.Id)
                                                    && role.Name == "Student"
                                                    select new RegisteredStudents
                                                    {
                                                        FirstName = user.FirstName,
                                                        LastName = user.LastName,
                                                        RegisteredDate = user.RegisteredDate.ToString("dd/MM/yyyy"),
                                                        Status = user.Status,
                                                        StudentNumber = user.StudentNumber,
                                                        Email = user.Email,
                                                        TotalCourses = 0,
                                                        Phone = user.PhoneNumber,
                                                        TotalCertifications = _context.UserCertifications.Where(x => x.UserId == user.Id).Count(),
                                                        LastLogin = _context.LoginTrail.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).Select(x => x.LogTime).FirstOrDefault()

                                                    }).ToListAsync();

                    }
                }
               return enrollmentList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<EnrollmentDetails> SingleEnrollment(string userEmail)
        //{
        //    try
        //    {
        //        var result = await _userManager.FindByEmailAsync(userEmail);
        //        var enrollmentList = new EnrollmentDetails();
        //        if (result != null)
        //        {
        //            enrollmentList = new EnrollmentDetails
        //            {
        //                DateOfBirth = result.DateOfBirth.ToString("dd/MM/yyyy"),
        //                FirstName = result.FirstName,
        //                Gender = result.Gender,
        //                LastName = result.LastName,
        //                MiddleName = result.MiddleName,
        //                ReferralCode = result.ReferralCode,
        //                RegisteredDate = result.RegisteredDate.ToString("dd/MM/yyyy"),
        //                Status = result.Status,
        //                Address = result.Address + " " + await _addressServices.GetAddressByCityId(result.CityId)

        //            };

        //        }
        //        return enrollmentList;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public async Task<PaymentVM> TransactionLog()
        {
            try
            {
                var result = new List<AllPayments>();
                var PaidCourse = await (from uph in _context.UserPaymentHistory
                                        join ur in _context.UserRole on uph.UserId equals ur.UserId
                                        join u in _context.users on ur.UserId equals u.Id
                                        join r in _context.Role on ur.RoleId equals r.Id
                                        where r.Name == "Student" && uph.PaymentFor == paymentForEnums.Course
                                        select new AllPayments
                                        {
                                            Amount = uph.Amount.ToString("N"),
                                            Id = uph.Id,
                                            Payee = $"{u.FirstName} {u.LastName}",
                                            PaymentDate = uph.PaymentDate.ToString("dd/MM/yyyy"),
                                            PaymentMethodId = uph.PaymentMethodId,
                                            PaymentReference = uph.PaymentRef,
                                            Program = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == uph.UserPaymentForId).Select(c => c.CoursePriceOption.Course.Category.Institution.ShortName + "/" + c.CoursePriceOption.Course.Category.Name + "/" + c.CoursePriceOption.Course.CourseCode).FirstOrDefault(),
                                            StatusId = uph.StatusId,
                                            TransactionReference = uph.Description,
                                            StudentId = uph.User.StudentNumber

                                        }).ToListAsync();


                //   await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student").Select(x => 

                if (PaidCourse.Count() > 0)
                {
                    result.AddRange(PaidCourse);
                }

                var PaidCert = await (from uph in _context.UserPaymentHistory
                                      join ur in _context.UserRole on uph.UserId equals ur.UserId
                                      join u in _context.users on ur.UserId equals u.Id
                                      join r in _context.Role on ur.RoleId equals r.Id
                                      where r.Name == "Student" && uph.PaymentFor == paymentForEnums.Certifications
                                      select new AllPayments
                                      {
                                          Amount = uph.Amount.ToString("N"),
                                          Id = uph.Id,
                                          Payee = $"{u.FirstName} {u.LastName}",
                                          PaymentDate = uph.PaymentDate.ToString("dd/MM/yyyy"),
                                          PaymentMethodId = uph.PaymentMethodId,
                                          PaymentReference = uph.PaymentRef,
                                          Program = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == uph.UserPaymentForId).Select(c => c.CertificationPriceOption.Certification.Category.Institution.ShortName + "/" + c.CertificationPriceOption.Certification.Category.Name + "/" + c.CertificationPriceOption.Certification.ShortCode).FirstOrDefault(),
                                          StatusId = uph.StatusId,
                                          TransactionReference = uph.Description,
                                          StudentId = uph.User.StudentNumber

                                      }).ToListAsync();
                if (PaidCert.Count() > 0)
                {
                    result.AddRange(PaidCert);
                }

                // var allROles = await _roleManager.Roles
                //Today's Record
               
                var TodayR = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date
                                    select uph.Id).Distinct().CountAsync();

                var TodayF = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Failed
                                    select uph.Id).Distinct().CountAsync();

                var TodayS = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Paid
                                    select uph.Id).Distinct().CountAsync();

                var TodayP = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                    select uph.Id).Distinct().CountAsync();
                //.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                //var TodayF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                //var TodayS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                //var TodayP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                var todayTotalRecord = new ReportStat()
                {
                    Failed = $"{TodayF:n0}",
                    Pending = $"{TodayP:n0}",
                    Successful = $"{TodayS:n0}",
                    Total = $"{TodayR:n0}"
                };

                //This Week Record
                var TodayDayWeek = DateTime.Now.DayOfWeek.ToString();
                var WeekR = 0;
                var WeekF = 0;
                var WeekS = 0;
                var WeekP = 0;
                if (TodayDayWeek.ToLower() == "sunday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "monday")
                {

                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "tuesday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) 
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();
                }
                else if (TodayDayWeek.ToLower() == "wednesday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "thursday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "friday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();


                }
                else if (TodayDayWeek.ToLower() == "saturday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();


                }
                var weekTotalRecord = new ReportStat()
                {
                    Failed = $"{WeekF:n0}",
                    Pending = $"{WeekP:n0}",
                    Successful = $"{WeekS:n0}",
                    Total = $"{WeekR:n0}"
                };

                //This Month Record
                var MonthR = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month).Select(x => x.Id).Distinct().CountAsync();
                var MonthF = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Failed
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                var MonthS = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Paid
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                var MonthP = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Pending
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                var monthTotalRecord = new ReportStat()
                {
                    Failed = $"{MonthF:n0}",
                    Pending = $"{MonthP:n0}",
                    Successful = $"{MonthS:n0}",
                    Total = $"{MonthR:n0}"
                };

                var vm = new PaymentVM
                {
                    AllPayments = result.OrderByDescending(x => x.PaymentDate).ToList(),
                    MonthRe = monthTotalRecord,
                    TodayRe = todayTotalRecord,
                    WeekRe = weekTotalRecord
                };
                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<PaymentVM> Paymemts()
        {
            try
            {
                var result = new List<AllPayments>();
                var PaidCourse = await (from uph in _context.UserPaymentHistory
                                        join ur in _context.UserRole on uph.UserId equals ur.UserId
                                        join u in _context.users on ur.UserId equals u.Id
                                        join r in _context.Role on ur.RoleId equals r.Id
                                        where r.Name == "Student" && uph.PaymentFor == paymentForEnums.Course && uph.StatusId == PaymentStatusEnums.Paid
                                        select new AllPayments
                                        {
                                            Amount = uph.Amount.ToString("N"),
                                            Id = uph.Id,
                                            Payee = $"{u.FirstName} {u.LastName}",
                                            PaymentDate = uph.PaymentDate.ToString("dd/MM/yyyy"),
                                            PaymentMethodId = uph.PaymentMethodId,
                                            PaymentReference = uph.PaymentRef,
                                            Program = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == uph.UserPaymentForId).Select(c => c.CoursePriceOption.Course.Category.Institution.ShortName + "/" + c.CoursePriceOption.Course.Category.Name + "/" + c.CoursePriceOption.Course.CourseCode).FirstOrDefault(),
                                            StatusId = uph.StatusId,
                                            TransactionReference = uph.Description,
                                            StudentId = uph.User.StudentNumber

                                        }).ToListAsync();



                //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Role.Name == "Student" && x.StatusId == PaymentStatusEnums.Paid).Select(x => new AllPayments
                //{
                //    Amount = x.Amount.ToString("N"),
                //    Id = x.Id,
                //    Payee = $"{x.User.FirstName } {x.User.LastName }",
                //    PaymentDate = x.PaymentDate.ToString("dd/MM/yyyy"),
                //    PaymentMethodId = x.PaymentMethodId,
                //    PaymentReference = x.PaymentRef,
                //    Program = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == x.UserPaymentForId).Select(c => c.CoursePriceOption.Course.Category.Institution.ShortName + "/" + c.CoursePriceOption.Course.Category.Name + "/" + c.CoursePriceOption.Course.CourseCode).FirstOrDefault(),
                //    StatusId = x.StatusId,
                //    TransactionReference = x.Description,
                //    StudentId = x.User.StudentNumber

                //}).ToListAsync();

                if (PaidCourse.Count() > 0)
                {
                    result.AddRange(PaidCourse);
                }

                var PaidCert = await (from uph in _context.UserPaymentHistory
                                      join ur in _context.UserRole on uph.UserId equals ur.UserId
                                      join u in _context.users on ur.UserId equals u.Id
                                      join r in _context.Role on ur.RoleId equals r.Id
                                      where r.Name == "Student" && uph.PaymentFor == paymentForEnums.Certifications && uph.StatusId == PaymentStatusEnums.Paid
                                      select new AllPayments
                                      {
                                          Amount = uph.Amount.ToString("N"),
                                          Id = uph.Id,
                                          Payee = $"{u.FirstName} {u.LastName}",
                                          PaymentDate = uph.PaymentDate.ToString("dd/MM/yyyy"),
                                          PaymentMethodId = uph.PaymentMethodId,
                                          PaymentReference = uph.PaymentRef,
                                          Program = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == uph.UserPaymentForId).Select(c => c.CertificationPriceOption.Certification.Category.Institution.ShortName + "/" + c.CertificationPriceOption.Certification.Category.Name + "/" + c.CertificationPriceOption.Certification.ShortCode).FirstOrDefault(),
                                          StatusId = uph.StatusId,
                                          TransactionReference = uph.Description,
                                          StudentId = uph.User.StudentNumber

                                      }).ToListAsync(); 
                
                //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Role.Name == "Student" && x.StatusId == PaymentStatusEnums.Paid).Select(x => new AllPayments
                //{
                //    Amount = x.Amount.ToString("N"),
                //    Id = x.Id,
                //    Payee = $"{x.User.FirstName } {x.User.LastName }",
                //    PaymentDate = x.PaymentDate.ToString("dd/MM/yyyy"),
                //    PaymentMethodId = x.PaymentMethodId,
                //    PaymentReference = x.PaymentRef,
                //    Program = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == x.UserPaymentForId).Select(c => c.CertificationPriceOption.Certification.Category.Institution.ShortName + "/" + c.CertificationPriceOption.Certification.Category.Name + "/" + c.CertificationPriceOption.Certification.ShortCode).FirstOrDefault(),
                //    StatusId = x.StatusId,
                //    TransactionReference = x.Description,
                //    StudentId = x.User.StudentNumber

                //}).ToListAsync();
                if (PaidCert.Count() > 0)
                {
                    result.AddRange(PaidCert);
                }


                //Today's Record
                var TodayR = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date
                                    select uph.Id).Distinct().CountAsync();

                var TodayF = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Failed
                                    select uph.Id).Distinct().CountAsync();

                var TodayS = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Paid
                                    select uph.Id).Distinct().CountAsync();

                var TodayP = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                    select uph.Id).Distinct().CountAsync();
                //.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                //var TodayF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                //var TodayS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                //var TodayP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                var todayTotalRecord = new ReportStat()
                {
                    Failed = $"{TodayF:n0}",
                    Pending = $"{TodayP:n0}",
                    Successful = $"{TodayS:n0}",
                    Total = $"{TodayR:n0}"
                };

                //This Week Record
                var TodayDayWeek = DateTime.Now.DayOfWeek.ToString();
                var WeekR = 0;
                var WeekF = 0;
                var WeekS = 0;
                var WeekP = 0;
                if (TodayDayWeek.ToLower() == "sunday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date == DateTime.Now && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "monday")
                {

                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-1) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-1) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "tuesday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-2) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();
                }
                else if (TodayDayWeek.ToLower() == "wednesday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-3) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-3) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "thursday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-4) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-4) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                }
                else if (TodayDayWeek.ToLower() == "friday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-5) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-5) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();


                }
                else if (TodayDayWeek.ToLower() == "saturday")
                {
                    WeekR = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6)
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    WeekF = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Failed
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    WeekS = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Paid
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    WeekP = await (from uph in _context.UserPaymentHistory
                                   join ur in _context.UserRole on uph.UserId equals ur.UserId
                                   join u in _context.users on ur.UserId equals u.Id
                                   join r in _context.Role on ur.RoleId equals r.Id
                                   where r.Name == "Student" && uph.PaymentDate.Date == DateTime.Now.Date.AddDays(-6) && uph.StatusId == PaymentStatusEnums.Pending
                                   select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-2) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                    //WeekR = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date).Select(x => x.Id).Distinct().CountAsync();
                    //WeekF = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                    //WeekS = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                    //WeekP = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Date >= DateTime.Now.Date.AddDays(-6) && x.PaymentDate.Date <= DateTime.Now.Date && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();


                }
                var weekTotalRecord = new ReportStat()
                {
                    Failed = $"{WeekF:n0}",
                    Pending = $"{WeekP:n0}",
                    Successful = $"{WeekS:n0}",
                    Total = $"{WeekR:n0}"
                };

                //This Month Record
                var MonthR = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month).Select(x => x.Id).Distinct().CountAsync();
                var MonthF = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Failed
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Failed).Select(x => x.Id).Distinct().CountAsync();
                var MonthS = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Paid
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Id).Distinct().CountAsync();
                var MonthP = await (from uph in _context.UserPaymentHistory
                                    join ur in _context.UserRole on uph.UserId equals ur.UserId
                                    join u in _context.users on ur.UserId equals u.Id
                                    join r in _context.Role on ur.RoleId equals r.Id
                                    where r.Name == "Student" && uph.PaymentDate.Month == DateTime.Now.Date.Month && uph.StatusId == PaymentStatusEnums.Pending
                                    select uph.Id).Distinct().CountAsync(); //await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.User.Role.Name == "Student" && x.PaymentDate.Month == DateTime.Now.Date.Month && x.StatusId == PaymentStatusEnums.Pending).Select(x => x.Id).Distinct().CountAsync();

                var monthTotalRecord = new ReportStat()
                {
                    Failed = $"{MonthF:n0}",
                    Pending = $"{MonthP:n0}",
                    Successful = $"{MonthS:n0}",
                    Total = $"{MonthR:n0}"
                };

                var vm = new PaymentVM
                {
                    AllPayments = result,
                    MonthRe = monthTotalRecord,
                    TodayRe = todayTotalRecord,
                    WeekRe = weekTotalRecord
                };
                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<AllPayments>> PaymentbyUser(string userEmail)
        {
            try
            {
                var result = new List<AllPayments>();
                var PaidCourse = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Course && x.User.Email == userEmail).Select(x => new AllPayments
                {
                    Amount = x.Amount.ToString("N"),
                    Id = x.Id,
                    Payee = $"{x.User.FirstName } {x.User.LastName }",
                    PaymentDate = x.PaymentDate.ToString("dd/MM/yyyy"),
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentReference = x.PaymentRef,
                    Program = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == x.UserPaymentForId).Select(c => c.CoursePriceOption.Course.Category.Institution.ShortName + "/" + c.CoursePriceOption.Course.Category.Name + "/" + c.CoursePriceOption.Course.CourseCode).FirstOrDefault(),
                    StatusId = x.StatusId,
                    TransactionReference = x.Description,
                    StudentId = x.User.StudentNumber

                }).ToListAsync();

                if (PaidCourse.Count() > 0)
                {
                    result.AddRange(PaidCourse);
                }

                var PaidCert = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentFor == paymentForEnums.Certifications && x.User.Email == userEmail).Select(x => new AllPayments
                {
                    Amount = x.Amount.ToString("N"),
                    Id = x.Id,
                    Payee = $"{x.User.FirstName } {x.User.LastName }",
                    PaymentDate = x.PaymentDate.ToString("dd/MM/yyyy"),
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentReference = x.PaymentRef,
                    Program = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == x.UserPaymentForId).Select(c => c.CertificationPriceOption.Certification.Category.Institution.ShortName + "/" + c.CertificationPriceOption.Certification.Category.Name + "/" + c.CertificationPriceOption.Certification.ShortCode).FirstOrDefault(),
                    StatusId = x.StatusId,
                    TransactionReference = x.Description,
                    StudentId = x.User.StudentNumber

                }).ToListAsync();
                if (PaidCert.Count() > 0)
                {
                    result.AddRange(PaidCert);
                }

                //var result = await _context.UserPaymentHistory.Include(x => x.UserProgramOption).ThenInclude(x => x.User).Include(x => x.UserProgramOption).ThenInclude(x => x.ProgramOption).ThenInclude(x => x.Category).ThenInclude(x => x.Program).Where(x=>x.UserProgramOption.User.Email == userEmail).Select(x => new AllPayments
                //{
                //    Amount =x.Amount.ToString("N"),
                //    Id = x.Id,
                //    Payee = $"{x.UserProgramOption.User.FirstName } {x.UserProgramOption.User.LastName }",
                //    PaymentDate = x.PaymentDate.ToShortDateString(),
                //    PaymentMethodId = x.PaymentMethodId,
                //    PaymentReference = x.PaymentReference,
                //    Program = x.UserProgramOption.ProgramOption.Name,
                //    StatusId = x.StatusId,
                //    TransactionReference = x.TransactionReference
                //}).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<AllPayments> SinglePayment(int paymentId)
        {
            try
            {
                var result = new AllPayments();
                var paym = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.Id == paymentId).FirstOrDefaultAsync();
                if(paym != null)
                {
                    if(paym.PaymentFor == paymentForEnums.Course)
                    {
                        result = new AllPayments()
                        {
                            Amount = paym.Amount.ToString("N"),
                            Id = paym.Id,
                            Payee = $"{paym.User.FirstName } {paym.User.LastName }",
                            PaymentDate = paym.PaymentDate.ToString("dd/MM/yyyy"),
                            PaymentMethodId = paym.PaymentMethodId,
                            PaymentReference = paym.PaymentRef,
                            Program = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == paym.UserPaymentForId).Select(c => c.CoursePriceOption.Course.Category.Institution.ShortName + "/" + c.CoursePriceOption.Course.Category.Name + "/" + c.CoursePriceOption.Course.CourseCode).FirstOrDefault(),
                            StatusId = paym.StatusId,
                            TransactionReference = paym.Description,
                            StudentId = paym.User.StudentNumber
                        };
                    }
                    else if (paym.PaymentFor == paymentForEnums.Certifications)
                    {
                        result = new AllPayments()
                        {
                            Amount = paym.Amount.ToString("N"),
                            Id = paym.Id,
                            Payee = $"{paym.User.FirstName } {paym.User.LastName }",
                            PaymentDate = paym.PaymentDate.ToString("dd/MM/yyyy"),
                            PaymentMethodId = paym.PaymentMethodId,
                            PaymentReference = paym.PaymentRef,
                            Program = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).ThenInclude(c => c.Category).ThenInclude(c => c.Institution).Where(c => c.Id == paym.UserPaymentForId).Select(c => c.CertificationPriceOption.Certification.Category.Institution.ShortName + "/" + c.CertificationPriceOption.Certification.Category.Name + "/" + c.CertificationPriceOption.Certification.ShortCode).FirstOrDefault(),
                            StatusId = paym.StatusId,
                            TransactionReference = paym.Description,
                            StudentId = paym.User.StudentNumber
                        };
                    }
                }
               
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string> ConfirmSinglePayment(ConfirmPaymentVM dto)
        {
            try
            {
                var adminPin = await _context.BackOfficePIN.Where(x => x.PIN == dto.PIN && x.UserId == dto.AdminId).FirstOrDefaultAsync();
                if(adminPin == null)
                {
                    return "You cannot perform this task.";
                }

                var result = await _context.UserPaymentHistory.Where(x => x.Id == Convert.ToInt32(dto.Id)).FirstOrDefaultAsync();
                
                if (result != null)
                {
                    var allPay = await _context.UserPaymentHistory.Where(x => x.PaymentRef == result.PaymentRef).ToListAsync();
                    foreach (var item in allPay)
                    {
                        item.StatusId = PaymentStatusEnums.Paid;
                    }
                    

                    var promoHistory = await _context.UserPaymentHistory.Where(x => x.PaymentRef == result.PaymentRef && x.PaymentFor == paymentForEnums.Promo).FirstOrDefaultAsync();
                    if (promoHistory != null)
                    {
                        promoHistory.StatusId = PaymentStatusEnums.Paid;

                        var promoUsage = new PromoUsageHistory
                        {
                            PromoId = promoHistory.UserPaymentForId,
                            UserId = promoHistory.UserId
                        };

                        await _context.PromoUsageHistory.AddAsync(promoUsage);
                        _context.UserPaymentHistory.Update(promoHistory);
                        await _context.SaveChangesAsync();
                    }

                    _context.UserPaymentHistory.Update(result);

                    await _context.SaveChangesAsync();

                    
                    //Get user course payment history
                    //------------------------------
                    var totalCourseAmountPaid = await _context.UserPaymentHistory.Where(x => x.UserId == result.UserId && x.UserPaymentForId == result.UserPaymentForId && x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                    var Coursepromo = await _context.UserPaymentHistory.Where(x => x.UserId == result.UserId && x.UserPaymentForId == result.UserPaymentForId && x.PaymentFor == paymentForEnums.Promo && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                    totalCourseAmountPaid += Coursepromo;

                    //Get program total amount
                    //------------------------

                    float programTotalAmount = 0;
                    var det = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == result.UserPaymentForId).Select(x => new { amount = x.CoursePriceOption.Amount, courseId = x.CoursePriceOption.CourseId, Id = x.Id }).FirstOrDefaultAsync();
                    if (det != null)
                    {
                        programTotalAmount = det.amount;
                    }

                    if ((float)totalCourseAmountPaid < programTotalAmount)
                    {
                        var changedCourseAmount = await _context.UserPaymentHistory.Where(x => x.UserId == result.UserId && x.CourseOptionDateChanged == true && x.ChangedToUserPaymentForId == result.UserPaymentForId).SumAsync(x => x.Amount);
                        var tA = totalCourseAmountPaid + changedCourseAmount;
                        if ((float)tA < programTotalAmount)
                        {
                            var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == result.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                            userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;
                            _context.UserCourses.Update(userCourse);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == result.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                            userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                            _context.UserCourses.Update(userCourse);
                            await _context.SaveChangesAsync();
                            //return "Successful";                                    
                        }
                        //return "Successful";
                    }
                    else
                    {
                        var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == result.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                        userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                        _context.UserCourses.Update(userCourse);
                        await _context.SaveChangesAsync();
                        //return "Successful";                                    
                    }

                    var cert = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Where(x => x.UserId == result.UserId && x.Id == result.UserPaymentForId).FirstOrDefaultAsync();
                    if (cert != null)
                    {
                        cert.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                        _context.UserCertifications.Update(cert);

                    }

                    var data = await _context.UserData.Include(x => x.Data).Where(x => x.UserId == result.UserId && x.Id == result.UserPaymentForId).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                        _context.UserData.Update(data);

                    }

                    var modem = await _context.UserDevices.Where(x => x.UserId == result.UserId && x.Id == result.UserPaymentForId).FirstOrDefaultAsync();
                    if (modem != null)
                    {
                        modem.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                        _context.UserDevices.Update(modem);

                    }

                    //Manage Referral Code
                    //--------------------
                    var referralCodeUsed = await _context.UserReferred.Where(x => x.ReferredUserId == result.UserId).FirstOrDefaultAsync();
                    if (referralCodeUsed != null)
                    {
                        var ActualReferredUserDis = await _context.UserPaymentHistory.Where(x => x.UserId == result.UserId && x.UserPaymentForId == det.Id && x.PaymentMethodId == PaymentMethodEnums.Referral).Select(x => x.Amount).FirstOrDefaultAsync();
                        decimal amountLeftToPay = (decimal)programTotalAmount - ActualReferredUserDis;

                        var CourseAmtPaid = await _context.UserPaymentHistory.Where(x => x.UserId == result.UserId && x.UserPaymentForId == det.Id && x.PaymentMethodId != PaymentMethodEnums.Referral && x.PaymentRef == result.PaymentRef).Select(x => x.Amount).FirstOrDefaultAsync();
                        var percPaidOfamountLeftToPay = (CourseAmtPaid * 100) / amountLeftToPay;

                        var ActualReferralDis = (referralCodeUsed.ReferralDiscount * programTotalAmount) / 100;

                        var EachReferralEarning = (percPaidOfamountLeftToPay * Convert.ToInt32(ActualReferralDis)) / 100;

                        var userRPH = await _context.UserReferralPaymentHistory.Include(x => x.UserRefer).Where(x => x.PaymentRef == result.PaymentRef && x.UserCourseId == det.Id && x.UserRefer.ReferredUserId == result.UserId).FirstOrDefaultAsync();
                        if (userRPH != null)
                        {
                            userRPH.Earning = (float)EachReferralEarning;

                            _context.UserReferralPaymentHistory.Update(userRPH);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await _context.SaveChangesAsync();
                    return "Successful";

                }
                return "Error Occured";


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ProgramCategory>> ProgramCategories()
        {
            try
            {
                var result = await _context.ProgramCategory.Include(x=>x.Institution).OrderByDescending(x=>x.Id).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ProgramCategory>> ProgramCategoriesByInstitutionId(int InstitutionId)
        {
            try
            {
                var result = await _context.ProgramCategory.Include(x => x.Institution).Where(x=>x.InstitutionId == InstitutionId).OrderByDescending(x=>x.Id).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Programs>> Programs()
        {
            try
            {
                var result = await _context.Programs.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Currency>> Currencys()
        {
            try
            {
                var result = await _context.Currency.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ProgramOptions>> ProgramOptions()
        {
            try
            {
                var result = await _context.ProgramOptions.Include(x => x.Category).ThenInclude(x=>x.Institution).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<programFees>> ProgramFees()
        {
            try
            {
                var result = await _context.ProgramOptions.Select(x => new programFees
                {
                    DepositNGN =x.DepositNGN,
                    Id =x.Id,
                    Name = x.Name,
                    PriceUSD =x.PriceUSD,
                    PriceNGN =x.PriceNGN,
                    DepositUSD =x.DepositUSD
                }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Programs>> AddPrograms(Programs dto)
        {
            try
            {
                var exist = await _context.Programs.Where(x=>x.Name == dto.Name).CountAsync();
                if(exist <= 0)
                {
                    var newPro = new Programs
                    {
                       // DepositNGN = dto.DepositNGN,
                       // CategoryId = dto.CategoryId,
                        //Duration =dto.Duration,
                        Name = dto.Name,
                       // PriceNGN =dto.PriceNGN,
                        Description = dto.Description,
                       // PriceUSD =dto.PriceUSD,
                       // DepositUSD =dto.DepositUSD
                    };

                    await _context.Programs.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }

                var result = await _context.Programs.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ProgramOptions>> AddProgramOption(ProgramOptions dto)
        {
            try
            {
                var exist = await _context.ProgramOptions.Where(x => x.Name == dto.Name && x.CategoryId == dto.CategoryId).CountAsync();
                if (exist <= 0)
                {
                    var lastId = await _context.ProgramOptions.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
                    
                    lastId = lastId + 1;
                    var newPro = new ProgramOptions
                    {
                        DepositNGN = dto.DepositNGN,
                        CategoryId = dto.CategoryId,
                        Duration =dto.Duration,
                        Name = dto.Name,
                        PriceNGN =dto.PriceNGN,
                        Description = dto.Description,
                        PriceUSD =dto.PriceUSD,
                        DepositUSD =dto.DepositUSD,
                        StartDate =dto.StartDate,
                        MaxSubjectSelection = dto.MaxSubjectSelection,
                        ProgramOptionId= $"{lastId:0000}"
                    };

                    await _context.ProgramOptions.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }

                var result = await _context.ProgramOptions.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ProgramCategory>> AddProgramCategory(ProgramCategory dto)
        {
            try
            {
                var exist = await _context.ProgramCategory.Where(x => x.Name == dto.Name && x.InstitutionId == dto.InstitutionId).CountAsync();
                if (exist <= 0)
                {
                    var newPro = new ProgramCategory
                    {
                        InstitutionId =dto.InstitutionId,
                        Name = dto.Name,
                        Description =dto.Description
                    };

                    await _context.ProgramCategory.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }
                var result = await _context.ProgramCategory.Include(x=>x.Institution).OrderByDescending(x=>x.Id).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Cour>> Courses()
        {
            try
            {
                var result = await _context.Courses.Include(x => x.Category).ThenInclude(x=>x.Institution).Select(x => new Cour
                {
                    Id = x.Id,
                    Category = x.Category.Name,
                   // Program = x.Category.Program.Name,
                    Name = x.Name,
                    Description = x.Description,
                    InstitutionCode = x.Category.Institution.ShortName,
                    CourseCode=x.CourseCode,
                    CategoryId =x.CategoryId,
                    InstitutionId =x.Category.InstitutionId
                    // StartDate = x.StartDate.ToString("dd/MM/yyyy"),
                    // Amount = x.Amount.ToString("N")
                }).ToListAsync();



                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Cour>> EditCourse(EditCourseDTO dto)
        {
            try
            {
                var exist = await _context.Courses.Where(x => x.Id == dto.CourseId).FirstOrDefaultAsync();
                if (exist != null)
                {
                    exist.CategoryId = dto.CourseDetails.CategoryId;
                    exist.CourseCode = dto.CourseDetails.CourseCode;
                    exist.Description = dto.CourseDetails.Description;
                    exist.Name = dto.CourseDetails.Name;
                   // exist.InstitutionId = dto.CourseDetails.InstitutionId;

                    _context.Courses.Update(exist);

                    await _context.SaveChangesAsync();
                }

                var result = await _context.Courses.Include(x => x.Category).ThenInclude(x => x.Institution).Select(x => new Cour
                {
                    Id = x.Id,
                    Category = x.Category.Name,
                    //Program = x.Category.Program.Name,
                    Name = x.Name,
                    Description = x.Description,
                    InstitutionCode = x.Category.Institution.ShortName,
                    CourseCode = x.CourseCode,
                    CategoryId = x.CategoryId,
                    InstitutionId = x.Category.InstitutionId
                    //StartDate = x.StartDate.ToString("dd/MM/yyyy"),
                    //Amount =x.Amount.ToString("N")

                }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<CoursePriceOptionVM> CourseOptions(int CourseId)
        {
            try
            {
                var result = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.CourseId == CourseId).Select(x=> new CoursePriceOptionV { Amount=x.Amount.ToString("N"), Course =x.Course.Name,Duration =x.Duration,Id =x.Id,Name =x.Name, EndDate = x.EndDate.ToString("dd/MM/yyyy"), StartDate = x.StartDate.ToString("dd/MM/yyyy")}).ToListAsync();
                var vm = new CoursePriceOptionVM();
                vm.CoursePriceOptionV = result;
                if (result.Count()>0)
                {
                    foreach(var item in result )
                    {
                        vm.courseName = item.Course;
                        vm.courseId = CourseId;
                        break;
                    }
                }

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<CoursePriceOptionVM> EditCourseOption(EditCourseOptionDTO dto)
        {
            try
            {
                var result = await _context.CoursePriceOptions.Where(x => x.Id == dto.OptionId).FirstOrDefaultAsync();
                if (result != null)
                {
                    result.Name = dto.coursePriceOptions.Name;
                    result.Amount = dto.coursePriceOptions.Amount;
                    result.Duration  = dto.coursePriceOptions.Duration;
                    result.EndDate  = dto.coursePriceOptions.EndDate;
                    result.StartDate = dto.coursePriceOptions.StartDate;

                    _context.CoursePriceOptions.Update(result);
                    await _context.SaveChangesAsync();

                }

                var result1 = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.CourseId == result.CourseId).Select(x => new CoursePriceOptionV { Amount = x.Amount.ToString("N"), Course = x.Course.Name, Duration = x.Duration, Id = x.Id, Name = x.Name, EndDate = x.EndDate.ToString("dd/MM/yyyy"), StartDate = x.StartDate.ToString("dd/MM/yyyy") }).ToListAsync();
                var vm = new CoursePriceOptionVM();
                vm.CoursePriceOptionV = result1;
                if (result1.Count() > 0)
                {
                    foreach (var item in result1)
                    {
                        vm.courseName = item.Course;
                        vm.courseId = result.CourseId;
                        break;
                    }
                }

                return vm;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Cour>> AddCourse(Courses dto)
        {
            try
            {
                var exist = await _context.Courses.Where(x => x.Name == dto.Name && x.CategoryId == dto.CategoryId).CountAsync();
                if (exist <= 0)
                {
                    var newPro = new Courses
                    {
                        CategoryId =dto.CategoryId,
                        Name = dto.Name,
                        Description=dto.Description,
                      //  InstitutionId = dto.InstitutionId,
                        CourseCode =dto.CourseCode
                       // StartDate =dto.StartDate,
                       // Amount =dto.Amount
                    };

                    await _context.Courses.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }
                var result = await _context.Courses.Include(x=>x.Category).ThenInclude(x=>x.Institution).Select(x => new Cour
                {
                    Id = x.Id,
                    Category = x.Category.Name,
                    //Program = x.Category.Program.Name,
                    Name = x.Name,
                    Description = x.Description,
                    InstitutionCode = x.Category.Institution.ShortName,
                    CourseCode = x.CourseCode,
                    CategoryId = x.CategoryId,
                    InstitutionId = x.Category.InstitutionId
                    //StartDate = x.StartDate.ToString("dd/MM/yyyy"),
                    //Amount =x.Amount.ToString("N")

                }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<CoursePriceOptionVM> AddCoursePrice(CoursePriceOptions dto)
        {
            try
            {
                var exist = await _context.CoursePriceOptions.Where(x => x.Name == dto.Name && x.CourseId == dto.CourseId && x.Duration == dto.Duration).CountAsync();
                if (exist <= 0)
                {
                    var newPro = new CoursePriceOptions
                    {
                        CourseId = dto.CourseId,
                        Name = dto.Name,
                        Duration = dto.Duration,
                        StartDate =dto.StartDate,
                        EndDate =dto.EndDate,
                        Amount =dto.Amount
                    };

                    await _context.CoursePriceOptions.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }

                var result = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.CourseId == dto.CourseId).Select(x => new CoursePriceOptionV { Amount = x.Amount.ToString("N"), Course = x.Course.Name, Duration = x.Duration, Id = x.Id, Name = x.Name, EndDate = x.EndDate.ToString("dd/MM/yyyy"), StartDate = x.StartDate.ToString("dd/MM/yyyy")}).ToListAsync();
                var vm = new CoursePriceOptionVM();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        vm.courseName = item.Course;
                        vm.courseId = dto.CourseId;

                        break;
                    }
                    vm.CoursePriceOptionV = result;
                }

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Certi>> Certifications()
        {
            try
            {
                var result = await _context.Certifications.Include(x => x.Category).ThenInclude(x => x.Institution)
                    .Select(x=> new Certi {
                        //Amount = x.AmountDollar.ToString(),
                        Id = x.Id,
                        Category = x.Category.Name,
                        Program = x.Category.Institution.ShortName,
                        Mode = x.Mode,
                        Name  = x.Name,
                        OrganisationName =x.OrganisationName,
                        ShortCode =x.ShortCode,
                        //Currency = x.Currency
                    }).ToListAsync();
                //if(result.Count () >0)
                //{
                //    foreach (var item in result)
                //    {
                //        if(item.Currency == CurrencyEnums.Naira)
                //        {
                //            item.Amount = "₦ " + Convert.ToDouble(item.Amount).ToString("N");
                //        }
                //        else
                //        {
                //            item.Amount = "$ " + item.Amount;
                //        }
                //    }
                //}
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<CertPriceOptionVM> CertificationOptions(int CertId)
        {
            try
            {
                var result = await _context.CertificationPriceOptions.Include(x=>x.Currency).Include(x => x.Certification).Where(x => x.CertificationId == CertId).Select(x => new CertPriceOptionV { Amount = x.Amount.ToString(), ExamDate = x.ExamDate.ToString("dd/MM/yyyy"), Id = x.Id, Currency = x.Currency.major_symbol, certName=x.Certification.Name, Charges =x.Charges.ToString("N") }).ToListAsync();
                
                var vm = new CertPriceOptionVM();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        if (item.Currency == "₦")
                        {
                            item.Amount = item.Currency +" " + Convert.ToDouble(item.Amount).ToString("N");
                        }
                        else
                        {
                            item.Amount = item.Currency +" " + item.Amount;
                        }
                        vm.certName  = item.certName; 
                        item.Charges = "₦ " + Convert.ToDouble(item.Charges).ToString("N");
                    }
                }

                vm.CertPriceOptionV = result;
                vm.certId = CertId;
                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Certi>> AddCertification(Certifications dto)
        {
            try
            {
                var exist = await _context.Certifications.Where(x => x.Name == dto.Name && x.CategoryId == dto.CategoryId).CountAsync();
                if (exist <= 0)
                {
                    var newPro = new Certifications
                    {
                        CategoryId = dto.CategoryId,
                        Name = dto.Name,
                        //AmountDollar  = dto.AmountDollar,
                        Mode = dto.Mode,
                        OrganisationName=dto.OrganisationName,
                        ShortCode =dto.ShortCode,
                        //Currency = dto.Currency
                    };

                    await _context.Certifications.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }
                var result = await _context.Certifications.Include(x => x.Category).ThenInclude(x => x.Institution).Select(x => new Certi
                {
                    //Amount = x.AmountDollar.ToString(),
                    Id = x.Id,
                    Category = x.Category.Name,
                    Program = x.Category.Institution.ShortName,
                    Mode = x.Mode,
                    Name = x.Name,
                    OrganisationName = x.OrganisationName,
                    ShortCode = x.ShortCode,
                    //Currency = x.Currency
                }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<CertPriceOptionVM> AddCertificationPrice(CertificationPriceOptions dto)
        {
            try
            {
                var exist = await _context.CertificationPriceOptions.Where(x => x.ExamDate.Date == dto.ExamDate.Date && x.CertificationId == dto.CertificationId && x.Amount == dto.Amount && x.CurrencyId == x.CurrencyId).CountAsync();
                if (exist <= 0)
                {
                    var newPro = new CertificationPriceOptions
                    {
                        CertificationId = dto.CertificationId,
                        CurrencyId = dto.CurrencyId,
                        ExamDate = dto.ExamDate,
                        Amount = dto.Amount,
                        Charges =dto.Charges
                    };

                    await _context.CertificationPriceOptions.AddAsync(newPro);

                    await _context.SaveChangesAsync();
                }

                var result = await _context.CertificationPriceOptions.Include(x=>x.Currency).Include(x => x.Certification).Where(x => x.CertificationId == dto.CertificationId).Select(x => new CertPriceOptionV { Amount = x.Amount.ToString(), ExamDate = x.ExamDate.ToString("dd/MM/yyyy"), Id = x.Id, Currency = x.Currency.major_symbol, certName = x.Certification.Name, Charges = x.Charges.ToString("N") }).ToListAsync();
                var vm = new CertPriceOptionVM();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        if (item.Currency == "₦")
                        {
                            item.Amount = item.Currency+" " + Convert.ToDouble(item.Amount).ToString("N");
                        }
                        else
                        {
                            item.Amount = item.Currency+" " + item.Amount;
                        }
                        vm.certName = item.certName;
                        item.Charges = "₦ " + Convert.ToDouble(item.Charges).ToString("N");

                    }
                }

                vm.CertPriceOptionV = result;

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
