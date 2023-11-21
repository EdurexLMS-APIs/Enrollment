using CPEA.Data;
using CPEA.Models;
using CPEA.Utilities.DTO;
using CPEA.Utilities.Interface;
using CPEA.Utilities.InterswitchClasses;
using CPEA.Utilities.Monnify;
using CPEA.Utilities.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace CPEA.Utilities.Services
{
    public class ProjectServices : IProjectServices
    {
        private readonly ApplicationDbContext _context;
        //private readonly IAddressServices _addressServices;
        private readonly APISettings _apiSettings;
        private readonly UserManager<Users> _userManager;
        //private readonly IMessagingService _messagingService;
        private readonly SignInManager<Users> _signInManager;
        private IWebHostEnvironment _env;
        private readonly IInterSwitch _intswitchService;
        private readonly IEmail _smtpMail;

        public ProjectServices(IEmail smtpMail, IInterSwitch intswitchService, ApplicationDbContext context, IWebHostEnvironment env, UserManager<Users> userManager, SignInManager<Users> signInManager, IOptions<APISettings> apiSettings)
        {
            _smtpMail = smtpMail;
            _intswitchService = intswitchService;
            _context = context;
           // _addressServices = addressServices;
            _userManager = userManager;
            _apiSettings = apiSettings.Value;
           // _messagingService = messagingService;
            _env = env;
            _signInManager = signInManager;

        }
        public async Task<string> ResetPassword(PasswordResetDTO dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.email);

                if (user == null)
                {
                    return "No record found";
                }

                var newPassword = _userManager.PasswordHasher.HashPassword(user, dto.NewPassword);
                user.PasswordHash = newPassword;
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
                var res = await _userManager.UpdateAsync(user);

                if (res.Succeeded)
                {                    
                    return "Successful";
                }
                return "Not Successful";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string Encrypt(string plain)
        {
            string output = "";
            char[] readChar = plain.ToCharArray();
            for (int i = 0; i < readChar.Length; i++)
            {
                int no = Convert.ToInt32(readChar[i]) + 10;
                string r = Convert.ToChar(no).ToString();
                output += r;
            }
            return output;
        }
        public async Task<Tuple<string, bool>> ForgotPassword(string email, string url)
        {
            try
            {
                var confirmEmail = await _userManager.FindByEmailAsync(email);

                if(confirmEmail ==null)
                {
                    return new Tuple<string, bool>("No record found", false);
                }

                var webRoot = _env.WebRootPath;

                var pathToFile = _env.WebRootPath
                                + Path.DirectorySeparatorChar.ToString()
                                + "Emails"
                                + Path.DirectorySeparatorChar.ToString()
                                + "forgotpassword.html";

                var builder = new StringBuilder();

                using (var reader = File.OpenText(pathToFile))
                {
                    builder.Append(reader.ReadToEnd());
                }

                string query = $"/Home/ResetPassword";
                var fullUrl = url + query;
                var code = GenerateReferalCode();
                var encryptedValue = Encrypt($"{confirmEmail.Email} {code}");
                fullUrl = fullUrl+"/?token="+ encryptedValue;

                builder.Replace("{StudentName}", $"{confirmEmail.FirstName} {confirmEmail.LastName}" );
                builder.Replace("{Url}", fullUrl);

                string messageBody = builder.ToString();

                //return new Tuple<CourseRegConfirmDTO, string>(null, "Email not sent");

                var sendEm = await _smtpMail.SendEmail(confirmEmail.Email, messageBody, "Reset password request.");
                if (sendEm)
                {
                    return new Tuple<string, bool>("Successful", true);
                }
                else
                {
                    return new Tuple<string, bool>("Email not sent", false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<Tuple<CourseRegConfirmDTO, string>> SendRegConfirmation(string userId, int userCourseId, string Mode)
        {
            try
            {
                var CourseRegDe = new CourseRegConfirmDTO();

                var Course = await _context.UserCourses.Include(x=>x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == userId && x.Id == userCourseId).FirstOrDefaultAsync();
                if(Course != null)
                {
                    CourseRegDe.courseName = Course.CoursePriceOption.Course.Name;
                    CourseRegDe.endDate = Course.CoursePriceOption.EndDate.Date.ToString("dd/MM/yyyy");
                    CourseRegDe.startDate = Course.CoursePriceOption.StartDate.Date.ToString("dd/MM/yyyy");
                    CourseRegDe.studentId = Course.User.StudentNumber;
                    CourseRegDe.studentName = $"{Course.User.FirstName} {Course.User.LastName}";
                    CourseRegDe.courseCode = Course.CoursePriceOption.Course.CourseCode;

                    var payment = await _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.UserPaymentForId == Course.Id && x.StatusId == PaymentStatusEnums.Paid).ToListAsync();
                    var paymentRec = new List<CoursePayment>();
                    if (payment.Count()>0)
                    {
                        foreach (var item in payment)
                        {
                            var eachPayment = new CoursePayment
                            {
                                amountPaid = "₦ " + item.Amount.ToString("N"),
                                paymentDate =item.PaymentDate.Date.ToString("dd/MM/yyyy"),
                                paymentMethod = item.PaymentMethodId
                            };
                            paymentRec.Add(eachPayment);
                        }
                        CourseRegDe.totalAmountPaid = "₦ " + payment.Sum(x => x.Amount).ToString("N");
                        CourseRegDe.coursePayment = paymentRec;
                    }

                    if(Mode == "Email")
                    {
                        var webRoot = _env.WebRootPath;

                        var pathToFile = _env.WebRootPath
                                        + Path.DirectorySeparatorChar.ToString()
                                        + "Emails"
                                        + Path.DirectorySeparatorChar.ToString()
                                        + "CourseRegConfirmation.html";

                        var builder = new StringBuilder();

                        using (var reader = File.OpenText(pathToFile))
                        {
                            builder.Append(reader.ReadToEnd());
                        }

                        var p = "";

                        if(CourseRegDe.coursePayment.Count()>0)
                        {
                            foreach(var item in CourseRegDe.coursePayment)
                            {
                                var f = "<tr style='font: 8px Arial;'><td style='width: 40%; font: 8px Arial; padding:6px'>" + item.paymentMethod + "</td><td style='width: 30%; font: 8px Arial; padding:6px'>" + item.paymentDate + "</td><td style='width: 30%; font: 8px Arial; padding:6px; text-align:right'>" + item.amountPaid + "</td></tr>";
                                p = p + f;
                            }
                        }

                        string paymentR = "<table style='border-collapse: collapse;'><thead><tr style='border-bottom: 1px solid black;'><th style='width:40%; font: 10px Arial; padding:8px'>Payment Method</th><th style='width:30%; font: 10px Arial; padding:8px'>Payment Date</th><th style='width:30%; font: 10px Arial; padding:8px'>Amount</th></tr></thead>" +
                                           "<tbody>"+ p
                                        + "</tbody></table>";

                        builder.Replace("{StudentName}", CourseRegDe.studentName);
                        builder.Replace("{CourseName}", CourseRegDe.courseName);
                        builder.Replace("{StudentId}", CourseRegDe.studentId);

                        builder.Replace("{StartDate}", CourseRegDe.startDate);
                        builder.Replace("{EndDate}", CourseRegDe.endDate);


                        builder.Replace("{tblPaymentRecord}", paymentR);
                        
                        string messageBody = builder.ToString();

                        //return new Tuple<CourseRegConfirmDTO, string>(null, "Email not sent");

                        var sendEm = await _smtpMail.SendEmail(Course.User.Email, messageBody, "Course Registration Confirmation");
                        if (sendEm)
                        {
                            return new Tuple<CourseRegConfirmDTO, string>(null, "Successful");
                        }
                        else
                        {
                            return new Tuple<CourseRegConfirmDTO, string>(null, "Email not sent");
                        }
                    }
                    else
                    {
                        return new Tuple<CourseRegConfirmDTO, string>(CourseRegDe, "Download");
                    }
                }

                return new Tuple<CourseRegConfirmDTO, string>(null, "Not found");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public CourseRegConfirmDTO DownloadRegConfirmation(string userId, int userCourseId, string Mode)
        {
            try
            {
                var CourseRegDe = new CourseRegConfirmDTO();

                var Course = _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == userId && x.Id == userCourseId).FirstOrDefault();
                if (Course != null)
                {
                    CourseRegDe.courseName = Course.CoursePriceOption.Course.Name;
                    CourseRegDe.endDate = Course.CoursePriceOption.EndDate.Date.ToString("dd/MM/yyyy");
                    CourseRegDe.startDate = Course.CoursePriceOption.StartDate.Date.ToString("dd/MM/yyyy");
                    CourseRegDe.studentId = Course.User.StudentNumber;
                    CourseRegDe.studentName = $"{Course.User.FirstName} {Course.User.LastName}";
                    CourseRegDe.courseCode = Course.CoursePriceOption.Course.CourseCode;

                    var payment = _context.UserPaymentHistory.Where(x => x.PaymentFor == paymentForEnums.Course && x.UserPaymentForId == Course.Id && x.StatusId == PaymentStatusEnums.Paid).ToList();
                    var paymentRec = new List<CoursePayment>();
                    if (payment.Count() > 0)
                    {
                        foreach (var item in payment)
                        {
                            var eachPayment = new CoursePayment
                            {
                                amountPaid = "₦ " + item.Amount.ToString("N"),
                                paymentDate = item.PaymentDate.Date.ToString("dd/MM/yyyy"),
                                paymentMethod = item.PaymentMethodId
                            };
                            paymentRec.Add(eachPayment);
                        }
                        CourseRegDe.totalAmountPaid = "₦ " + payment.Sum(x => x.Amount).ToString("N");
                        CourseRegDe.coursePayment = paymentRec;
                    }
                    return CourseRegDe;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GeneratePaymentLog()
        {
            Random rd = new Random();

            string result = "";

            for (int i = 0; i < 6; i++)
            {
                result += rd.Next(10);
            }

            return result;
        }
        public async Task<string> GetPromoCode(string Code)
        {
            var perce = "";
            var codeDetails = await _context.Promo.Where(x => x.Code == Code).FirstOrDefaultAsync();
            if(codeDetails.StartDate.Date <= DateTime.Now.Date && codeDetails.EndDate.Date >= DateTime.Now.Date)
            {
                perce = codeDetails.PromoPercentage+"";
            }
            else
            {
                perce = "Promo has expired";
            }
            return perce;
        }

        public async Task<string> EnrollmentInterswitch(Register2DTO dto)
        {
            try
            {
                var paymentList = new List<PaymentDTO2>();
                var user = await _userManager.FindByIdAsync(dto.userId);
                if (user != null)
                {
                    
                    PaymentMethodEnums paymentM = new PaymentMethodEnums();
                    if (dto.UserPayment.PaymentMethod.ToLower() == "card")
                    {
                        paymentM = PaymentMethodEnums.Card;
                    }
                    else if (dto.UserPayment.PaymentMethod.ToLower() == "offline")
                    {
                        paymentM = PaymentMethodEnums.Offline;
                    }
                    else if (dto.UserPayment.PaymentMethod.ToLower() == "interswitch")
                    {
                        paymentM = PaymentMethodEnums.InterSwitch;
                    }

                    var paymentRef = "";
                    if (dto.UserPayment.OfflinePaymentRef == null && dto.UserPayment.OfflinePaymentRef == "")
                    {
                        paymentRef = $"{DateTime.Now.Ticks}{(int)paymentM}";
                    }
                    else
                    {
                        paymentRef = dto.UserPayment.OfflinePaymentRef;
                    }

                    if (dto.userCourseOption != null && dto.userCourseOption.CoursePriceOptionId > 0)
                    {
                        UserCourseStatusEnums status = new UserCourseStatusEnums();

                        var courseDate = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.Id == dto.userCourseOption.CoursePriceOptionId).Select(x => new { startD = x.StartDate.Date, endD = x.EndDate.Date }).FirstOrDefaultAsync();
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
                        //var certTypeId = 0;
                        //if (dto.userCourseOption.CertificateTypeId == 0)
                        //{
                        //    certTypeId = await _context.CertificateType.Where(x => x.CertType == "Electronic").Select(x => x.Id).FirstOrDefaultAsync();
                        //}
                        //else
                        //{
                        //    certTypeId = dto.userCourseOption.CertificateTypeId;
                        //}

                        var newCourseOp = new UserCourses
                        {
                            CoursePriceOptionId = dto.userCourseOption.CoursePriceOptionId,
                            //CertificateTypeId = certTypeId,
                            deliveryStateId = dto.userCourseOption.deliveryStateId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                            Status = status
                        };

                        await _context.UserCourses.AddAsync(newCourseOp);
                        await _context.SaveChangesAsync();

                        var CourseDTO = new PaymentDTO2()
                        {
                            Amount = (decimal)dto.userCourseOption.AmountPaid,
                            PaymentFor = paymentForEnums.Course,
                            UserPaymentForId = newCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = dto.UserPayment.OfflinePaymentRef,
                            Description = "Course Fee"
                        };
                        paymentList.Add(CourseDTO);

                        if (dto.userCourseOption.CertificateTypeId != null && dto.userCourseOption.CertificateTypeId > 0)
                        {
                            var certType = await _context.CertificateType.Where(x => x.Id == dto.userCourseOption.CertificateTypeId).FirstOrDefaultAsync();
                            if (certType.CertType.ToLower() == "physical")
                            {
                                var ctDTO = new PaymentDTO2()
                                {
                                    Amount = (decimal)certType.Fee,
                                    PaymentFor = paymentForEnums.PhysicalCertificate,
                                    UserPaymentForId = newCourseOp.Id,
                                    UserId = dto.userId,
                                    paymentRef = dto.UserPayment.OfflinePaymentRef,
                                    Description = "Certificate Type Fee"
                                };
                                paymentList.Add(ctDTO);
                            }
                        }

                        if (dto.userCourseOption.deliveryStateId != null && dto.userCourseOption.deliveryStateId > 0)
                        {
                            var courierFee = await _context.States.Where(x => x.Id == dto.userCourseOption.deliveryStateId).Select(x => x.CourierFee).FirstOrDefaultAsync();
                            var dsDTO = new PaymentDTO2()
                            {
                                Amount = (decimal)courierFee,
                                PaymentFor = paymentForEnums.Courier,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = dto.UserPayment.OfflinePaymentRef,
                                Description = "Courier Fee"
                            };
                            paymentList.Add(dsDTO);
                        }

                        //var userCoursePayment = new UserPaymentHistory
                        //{
                        //    Amount = (decimal)dto.userCourseOption.AmountPaid,
                        //    PaymentDate = DateTime.Now,
                        //    PaymentFor = paymentForEnums.Course,
                        //    PaymentMethodId = paymentM,
                        //    StatusId = PaymentStatusEnums.Initialized,
                        //    UserId = dto.userId,
                        //    UserPaymentForId = newCourseOp.Id,
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

                        var rateAmount = await GetCertificationConvertedValuebyCertOptId(dto.userCertificationOption.CertificationPriceOptionId);
                        var amount = rateAmount.Split(',')[0];
                        var rate = rateAmount.Split(',')[1];
                        var charges = rateAmount.Split(',')[2];

                        var pDTO = new PaymentDTO2()
                        {
                            Amount = Convert.ToDecimal(amount) * Convert.ToDecimal(rate) + Convert.ToDecimal(charges),
                            PaymentFor = paymentForEnums.Certifications,
                            UserPaymentForId = newCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = dto.UserPayment.OfflinePaymentRef,
                            Description = "Certificate Fee"
                        };
                        paymentList.Add(pDTO);
                    }

                    if (dto.userDataOption != null && dto.userDataOption.DataId > 0)
                    {
                        var newCourseOp = new UserData
                        {
                            DataId = dto.userDataOption.DataId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                        };

                        await _context.UserData.AddAsync(newCourseOp);

                        await _context.SaveChangesAsync();

                        decimal dataFee = 0;
                        var strdataFee = await _context.tblData.Where(x => x.Id == dto.userDataOption.DataId).Select(x => x.Amount).FirstOrDefaultAsync();
                        if (strdataFee.ToLower() != "free")
                        {
                            dataFee = Convert.ToDecimal(strdataFee);

                            var pDTO = new PaymentDTO2()
                            {
                                Amount = dataFee,
                                PaymentFor = paymentForEnums.Data,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = dto.UserPayment.OfflinePaymentRef,
                                Description = "Data Fee"
                            };
                            paymentList.Add(pDTO);
                        }

                    }

                    if (dto.userModemOption != null && dto.userModemOption.ModemId > 0)
                    {
                        var newCourseOp = new UserDevices
                        {
                            Type = "Modem",
                            TypeId = dto.userModemOption.ModemId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                        };

                        await _context.UserDevices.AddAsync(newCourseOp);
                        await _context.SaveChangesAsync();

                        decimal modemFee = 0;
                        var strmodemFee = await _context.tblModem.Where(x => x.Id == dto.userModemOption.ModemId).Select(x => x.Amount).FirstOrDefaultAsync();
                        if (strmodemFee.ToLower() != "free")
                        {
                            modemFee = Convert.ToDecimal(strmodemFee);

                            var pDTO = new PaymentDTO2()
                            {
                                Amount = modemFee,
                                PaymentFor = paymentForEnums.Modem,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = dto.UserPayment.OfflinePaymentRef,
                                Description = "Modem Fee"
                            };
                            paymentList.Add(pDTO);
                        }

                    }

                    decimal totalAmount = 0;
                    totalAmount =paymentList.Sum(x => x.Amount);
                    //Calling Interswitch Payment
                    var Interswitchpayment = new InterswitchPaymentHistory
                    {
                        Amount = totalAmount,
                        ItemCode = paymentRef,
                        Status = PaymentStatusEnums.Initialized,
                        PaymentDate = DateTime.Now,
                        UserId = dto.userId,
                        //PaymentLogId = PaymentLogId
                    };

                    await _context.InterswitchPaymentHistory.AddAsync(Interswitchpayment);
                    await _context.SaveChangesAsync();
                    if (paymentList.Count >0)
                    {
                       // decimal changedCourseAmountPaid = 0;
                        UserPaymentHistory paymentHistroyDis = new UserPaymentHistory();
                        foreach (var item in paymentList)
                        {
                            float amount = 0;
                            if (item.UserOldPaymentForId > 0)
                            {
                                var oldPaymentCourse = await _context.UserPaymentHistory.Where(x => x.UserId == item.UserId && x.UserPaymentForId == item.UserOldPaymentForId).ToListAsync();
                                if (oldPaymentCourse.Count() > 0)
                                {
                                    var courseOnly = oldPaymentCourse.Where(x => x.PaymentFor == paymentForEnums.Course).ToList();
                                    var othersOnly = oldPaymentCourse.Where(x => x.PaymentFor != paymentForEnums.Course).ToList();
                                    foreach (var rec in courseOnly)
                                    {
                                        rec.ChangedToUserPaymentForId = item.UserPaymentForId;
                                        rec.CourseOptionDateChanged = true;
                                        _context.UserPaymentHistory.Update(rec);

                                        item.Description = "Change course date";
                                    }

                                    foreach (var rec1 in othersOnly)
                                    {
                                        rec1.UserPaymentForId = item.UserPaymentForId;
                                        _context.UserPaymentHistory.Update(rec1);
                                    }

                                    await _context.SaveChangesAsync();
                                }
                                amount = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == item.UserPaymentForId).Select(x => x.CoursePriceOption.Amount).FirstOrDefaultAsync();

                            }
                            else
                            {
                                amount = (float)item.Amount;
                            }

                            //userId = item.UserId;
                            paymentHistroyDis = new UserPaymentHistory
                            {
                                Amount = (decimal)amount,
                                PaymentRef = paymentRef,
                                StatusId = PaymentStatusEnums.Initialized,
                                PaymentMethodId = paymentM,
                                PaymentDate = DateTime.Now,
                                PaymentFor = item.PaymentFor,
                                UserId = item.UserId,
                                UserPaymentForId = item.UserPaymentForId,
                                Description =item.Description
                            };
                            _context.UserPaymentHistory.Add(paymentHistroyDis);
                        }

                        await _context.SaveChangesAsync();

                        //Redirect to interswitch payment page;
                                             

                        var result = "";// await _intswitchService.PaymentValidation(Parameters);
                        
                        if(result == "Successful")
                        {
                            return "Payment was successful.";
                        }
                        else
                        {
                            return "Payment failed.";
                        }
                    }

                    return "No payment selected";

                }
                return "Invalid user";
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<WalletRecord> GetWallet(string UserId)
        {
            var wallet = await _context.UserWalletHistory.Include(x=>x.Wallet).Where(x=>x.Wallet.UserId == UserId).Select(x=> new WalletRecordList
            { 
                Amount= "₦ " + x.Amount.ToString("N"),
                PaymentDate = x.PaymentDate.ToString("dd/MM/yyyy"),
                Type = x.TransactionType
            }).ToListAsync();

            var uWall = await _context.UserWallet.Where(x => x.UserId == UserId).FirstOrDefaultAsync();
            var wall = new WalletRecord
            {
                WalletRecordList = wallet,
                WalletId = uWall.WalletId,
                AvailableBalance = "₦ " + uWall.AvailableBalance.ToString("N"),
            };

            return wall;
        }

        public async Task<PaymentInitialiseResponse> ChangeCourseDate(ChangeCDate dto, string redirectURL)
        {
            try
            {
                var paymentList = new List<PaymentDTO2>();
                var user = await _userManager.FindByIdAsync(dto.userId);
                if (user != null)
                {
                    

                    if (dto.paymentDTO.Amount > 0)
                    {
                        //var oldCourseOption = dto.UserCourseId;
                        var CurrentUserCourse = new UserCourses();
                        var NewUserCourseOp = new UserCourses();
                        if (dto.NewCourseOptionId != null && dto.NewCourseOptionId > 0)
                        {

                            UserCourseStatusEnums status = new UserCourseStatusEnums();

                            var NewcourseDate = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.Id == dto.NewCourseOptionId).Select(x => new { startD = x.StartDate.Date, endD = x.EndDate.Date }).FirstOrDefaultAsync();
                            if (NewcourseDate != null)
                            {
                                CurrentUserCourse = await _context.UserCourses.Where(x => x.UserId == dto.userId && x.Id == dto.UserCourseId).FirstOrDefaultAsync();
                                if (CurrentUserCourse != null)
                                {
                                    CurrentUserCourse.Status = UserCourseStatusEnums.Canceled;
                                    _context.UserCourses.Update(CurrentUserCourse);
                                }

                                if (NewcourseDate.startD > DateTime.Now.Date)
                                {
                                    status = UserCourseStatusEnums.Pending;
                                }
                                else if (NewcourseDate.startD <= DateTime.Now.Date && NewcourseDate.endD >= DateTime.Now.Date)
                                {
                                    status = UserCourseStatusEnums.InProgress;
                                }
                                else if (NewcourseDate.endD < DateTime.Now.Date)
                                {
                                    status = UserCourseStatusEnums.Completed;
                                }

                                NewUserCourseOp = new UserCourses
                                {
                                    CoursePriceOptionId = dto.NewCourseOptionId,
                                    CertificateTypeId = CurrentUserCourse.CertificateTypeId,
                                    deliveryStateId = CurrentUserCourse.deliveryStateId,
                                    RegisteredDate = DateTime.Now,
                                    UserId = dto.userId,
                                    Status = status
                                };

                                await _context.UserCourses.AddAsync(NewUserCourseOp);
                                await _context.SaveChangesAsync();


                                var cangeCouD = new UserCourseDateChanged
                                {
                                    ChangedDate =DateTime.Now,
                                    ChangedFromId = dto.UserCourseId,
                                    ChangedToId = NewUserCourseOp.Id,
                                    UserId = dto.userId
                                };

                                await _context.UserCourseDateChanged.AddAsync(cangeCouD);
                            }


                            await _context.SaveChangesAsync();
                        }

                        var pDTO = new PaymentDTO2()
                        {
                            Amount = (decimal)dto.paymentDTO.Amount,
                            PaymentFor = paymentForEnums.Course,
                            UserPaymentForId = NewUserCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = dto.paymentDTO.paymentRef,
                            UserOldPaymentForId = CurrentUserCourse.Id
                        };
                        paymentList.Add(pDTO);

                        PaymentMethodEnums paymentM = new PaymentMethodEnums();
                        if (dto.paymentDTO.PaymentMethod.ToLower() == "card")
                        {
                            paymentM = PaymentMethodEnums.Card;
                        }
                        else if (dto.paymentDTO.PaymentMethod.ToLower() == "offline")
                        {
                            paymentM = PaymentMethodEnums.Offline;
                        }

                        switch (paymentM)
                        {
                            case PaymentMethodEnums.Card:
                                var cardResponse = await InitializeCardPayment2(paymentList, paymentM, redirectURL);
                                return cardResponse;
                            case PaymentMethodEnums.Offline:
                                var OfflineResponse = await InitializeOfflinePayment2(paymentList, redirectURL);
                                return OfflineResponse;

                        }
                        throw new Exception("No subscription method found");
                    }
                    else if (dto.paymentDTO.Amount < 0)
                    {
                        dto.paymentDTO.Amount = dto.paymentDTO.Amount * -1;
                        PaymentMethodEnums paymentM = new PaymentMethodEnums();

                        if (dto.NewCourseOptionId != null && dto.NewCourseOptionId > 0)
                        {
                            UserCourseStatusEnums status = new UserCourseStatusEnums();
                            var CurrentUserCourse = new UserCourses();
                            var NewUserCourseOp = new UserCourses();

                            var courseDate = await _context.CoursePriceOptions.Include(x => x.Course).Where(x => x.Id == dto.NewCourseOptionId).Select(x => new { startD = x.StartDate.Date, endD = x.EndDate.Date, price = x.Amount }).FirstOrDefaultAsync();
                            if (courseDate != null)
                            {
                                CurrentUserCourse = await _context.UserCourses.Where(x => x.UserId == dto.userId && x.Id == dto.UserCourseId).FirstOrDefaultAsync();
                                if (CurrentUserCourse != null)
                                {
                                    CurrentUserCourse.Status = UserCourseStatusEnums.Canceled;
                                    _context.UserCourses.Update(CurrentUserCourse);
                                }

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

                                var newCourseOp = new UserCourses
                                {
                                    CoursePriceOptionId = dto.NewCourseOptionId,
                                    CertificateTypeId = CurrentUserCourse.CertificateTypeId,
                                    deliveryStateId = CurrentUserCourse.deliveryStateId,
                                    RegisteredDate = DateTime.Now,
                                    UserId = dto.userId,
                                    Status = status,
                                    PaymentStatus = UserProgramPaymentStatusEnums.Paid
                                };
                                await _context.UserCourses.AddAsync(newCourseOp);
                                await _context.SaveChangesAsync();


                                var cangeCouD = new UserCourseDateChanged
                                {
                                    ChangedDate = DateTime.Now,
                                    ChangedFromId = dto.UserCourseId,
                                    ChangedToId = newCourseOp.Id,
                                    UserId = dto.userId
                                };

                                await _context.UserCourseDateChanged.AddAsync(cangeCouD);

                                var oldPaymentHistory = await _context.UserPaymentHistory.Where(x => x.UserId == dto.userId && x.UserPaymentForId == CurrentUserCourse.Id).ToListAsync();
                                decimal oldTotalAmount = 0;
                                if(oldPaymentHistory.Count()>0)
                                {
                                    var oldCourseOnly = oldPaymentHistory.Where(x=>x.PaymentFor == paymentForEnums.Course).ToList();
                                    var oldPH = oldPaymentHistory.Where(x=>x.PaymentFor != paymentForEnums.Course).ToList();
                                    oldTotalAmount = oldCourseOnly.Sum(x => x.Amount);

                                    foreach (var item in oldCourseOnly)
                                    {
                                        item.CourseOptionDateChanged = true;
                                        item.ChangedToUserPaymentForId = newCourseOp.Id;

                                        _context.UserPaymentHistory.Update(item);
                                    }

                                    foreach (var item1 in oldPH)
                                    {
                                        item1.UserPaymentForId = newCourseOp.Id;

                                        _context.UserPaymentHistory.Update(item1);
                                    }

                                    await _context.SaveChangesAsync();

                                    foreach (var item in oldPaymentHistory)
                                    {
                                        paymentM = item.PaymentMethodId;
                                        break;
                                    }

                                }
                                var subRef = $"{DateTime.Now.Ticks}{(int)paymentM}";
                                var paymMethod = "";
                                if(paymentM == PaymentMethodEnums.Card)
                                {
                                    paymMethod = "Card";
                                }
                                else if (paymentM == PaymentMethodEnums.Offline)
                                {
                                    paymMethod = "Offline";
                                }

                                var paymentHistroyDis = new UserPaymentHistory
                                {
                                    Amount = (decimal)courseDate.price,
                                    PaymentRef = subRef,
                                    Description = "Changed Date",
                                    StatusId = PaymentStatusEnums.Paid,
                                    PaymentMethodId = paymentM,
                                    PaymentDate = DateTime.Now,
                                    PaymentFor = paymentForEnums.Course,
                                    UserId = dto.userId,
                                    UserPaymentForId = newCourseOp.Id
                                };
                                _context.UserPaymentHistory.Add(paymentHistroyDis);

                                await _context.SaveChangesAsync();

                                var wallet = await _context.UserWallet.Where(x => x.UserId == dto.userId).FirstOrDefaultAsync();
                                if (wallet != null)
                                {
                                    dto.paymentDTO.Amount = dto.paymentDTO.Amount;
                                    wallet.SavingBalance += dto.paymentDTO.Amount;
                                    wallet.AvailableBalance += dto.paymentDTO.Amount;

                                    _context.UserWallet.Update(wallet);

                                    var walletHist = new UserWalletHistory
                                    {
                                        Amount = dto.paymentDTO.Amount,
                                        TransactionType = WalletEnums.Credit,
                                        WalletId = wallet.Id,
                                        PaymentDate =DateTime.Now
                                    };

                                    await _context.UserWalletHistory.AddAsync(walletHist);

                                    await _context.SaveChangesAsync();
                                }
                            }
                            
                        }


                    }
                        // return new PaymentInitialiseResponse() { errorMessage = "Error creating user." };
                    var response = new PaymentInitialiseResponse()
                    {
                        checkOutURL = "Updated",
                    };
                    return response;
                }
                return new PaymentInitialiseResponse() { errorMessage = "Error creating user." };

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<Users> GetProfile(string userId)
        {
            //Load user courses
            //------------------
            var result = await _userManager.FindByIdAsync(userId);

            return result;
        }
        public async Task<List<UserCourseRecord>> StudentCourses(string userId)
        {
            //Load user courses
            //------------------
            var result = await _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.UserId == userId && x.Status != UserCourseStatusEnums.Canceled).OrderByDescending(x => x.Id).Select(x => new UserCourseRecord
            {
               // certificateType = x.CertificateType.CertType,
                CourierState = x.deliveryStateId.ToString(),
                CourseCode = x.CoursePriceOption.Course.CourseCode,
                Name = x.CoursePriceOption.Course.Name,
                Price = x.CoursePriceOption.Amount.ToString(),
                ProgramPaymentStatus = x.PaymentStatus,
                ProgramStatus = x.Status,
                StartDate = x.CoursePriceOption.StartDate.Date.ToString("dd/MM/yyyy"),
                userCourseId = x.Id,
                InstitutionCode = x.CoursePriceOption.Course.Category.Institution.ShortName,
                CourseId = x.CoursePriceOption.CourseId

            }).ToListAsync();
            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    item.Name = item.Name.Length <= 20 ? item.Name : item.Name.Substring(0, 20)+"...";

                    var amountPaid = _context.UserPaymentHistory.Where(c => c.UserId == userId && c.PaymentFor == paymentForEnums.Course && c.UserPaymentForId == item.userCourseId).Select(c => c.Amount).Sum();
                    if(Convert.ToDecimal(item.Price) > amountPaid)
                    {
                        var changedCourseAmount = await _context.UserPaymentHistory.Where(x => x.UserId == userId && x.CourseOptionDateChanged == true && x.ChangedToUserPaymentForId == item.userCourseId).SumAsync(x => x.Amount);
                        amountPaid += changedCourseAmount;
                    }

                    decimal owing = Convert.ToDecimal(item.Price) - amountPaid;
                    item.amountOwing = "₦ " + owing.ToString("N");

                    item.amountPaid = "₦ " + amountPaid.ToString("N");
                    item.Price = "₦ " + Convert.ToDouble(item.Price).ToString("N");


                }
            }
            return result;
        }
        public async Task<UserCourseRecord> SingleCourse(int Id, string userId)
        {
            //Load user courses
            //------------------
            var result = await _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.UserId == userId && x.Status != UserCourseStatusEnums.Canceled).OrderByDescending(x => x.Id).Select(x => new UserCourseRecord
            {
                //certificateType = x.CertificateType.CertType,
                CourierState = x.deliveryStateId.ToString(),
                CourseCode = x.CoursePriceOption.Course.CourseCode,
                Name = x.CoursePriceOption.Course.Name,
                Price = x.CoursePriceOption.Amount.ToString(),
                ProgramPaymentStatus = x.PaymentStatus,
                ProgramStatus = x.Status,
                StartDate = x.CoursePriceOption.StartDate.Date.ToString("dd/MM/yyyy"),
                userCourseId = x.Id,
                Institutio = x.CoursePriceOption.Course.Category.Institution.ShortName,
                CourseId = x.CoursePriceOption.CourseId,
                Category =x.CoursePriceOption.Course.Category.Name,
                //Program =x.CoursePriceOption.Course.Category.Program.Name,
                CertIssued =x.CertificateIssued,
                CourseOption =x.CoursePriceOption.Name,
                Description =x.CoursePriceOption.Course.Description,
                Duration =x.CoursePriceOption.Duration,
                EndDate =x.CoursePriceOption.EndDate.Date.ToString("dd/MM/yyyy"),
                RegDate =x.RegisteredDate.Date.ToString("dd/MM/yyyy"),

            }).FirstOrDefaultAsync();
            if (result != null)
            {
                var amountPaid = _context.UserPaymentHistory.Where(c => c.UserId == userId && c.PaymentFor == paymentForEnums.Course && c.UserPaymentForId == result.userCourseId).Select(c => c.Amount).Sum();
                if (Convert.ToDecimal(result.Price) > amountPaid)
                {
                    var changedCourseAmount = await _context.UserPaymentHistory.Where(x => x.UserId == userId && x.CourseOptionDateChanged == true && x.ChangedToUserPaymentForId == result.userCourseId).SumAsync(x => x.Amount);
                    amountPaid += changedCourseAmount;
                }

                decimal owing = Convert.ToDecimal(result.Price) - amountPaid;
                result.amountOwing = "₦ " + owing.ToString("N");

                result.amountPaid = "₦ " + amountPaid.ToString("N");
                result.Price = "₦ " + Convert.ToDouble(result.Price).ToString("N");

                if(result.CourierState != "" && result.CourierState != null)
                {
                    result.CourierState = await _context.States.Where(x => x.Id == Convert.ToInt32(result.CourierState)).Select(x => x.Name).FirstOrDefaultAsync();
                }

            }
            return result;
        }
        public async Task<List<UserCertificationsRecord>> StudentCertifications(string userId)
        {
            //Load user certifications
            //------------------
            var result = await _context.UserCertifications.Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserCertificationsRecord
            {
                CertificationMode = x.CertificationPriceOption.Certification.Mode,
                ExamDate = x.CertificationPriceOption.ExamDate.Date.ToString("dd/MM/yyyy"),
                Name = x.CertificationPriceOption.Certification.Name,
                Price = x.CertificationPriceOption.Amount.ToString(),
                currency = x.CertificationPriceOption.Currency.major_symbol,
                ProgramStatus = x.Status,
                ProgramPaymentStatus = x.PaymentStatus,
                ShortCode = x.CertificationPriceOption.Certification.ShortCode,
                userCertificationId = x.Id,
                CertOptId = x.CertificationPriceOptionId,
                provider = x.CertificationPriceOption.Certification.OrganisationName

            }).Take(5).ToListAsync();

            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    var rateAmount = await GetCertificationConvertedValuebyCertOptId(item.CertOptId);
                    var amount = rateAmount.Split(',')[0];
                    var rate = rateAmount.Split(',')[1];

                    if (item.currency != null && item.currency != "₦")
                    {
                        item.Price = "₦ " + (Convert.ToDouble(amount) * Convert.ToDouble(rate)).ToString("N");
                    }
                    else
                    {
                        item.Price = "₦ " + item.Price;
                    }
                }
            }

            return result;
        }
        public async Task<UserDevicesSubscriptionsVM> StudentDataDevices(string userId)
        {
            var data = await _context.UserData.Include(x => x.Data).Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserDataRecord
            {
                Amount = x.Data.Amount,
                Bundle = x.Data.Bundle,
                Validity = x.Data.Duration,
                NetworkProvider = x.Data.NetworkProvider,
                StartDate = x.RegisteredDate.Date.ToString("dd/MM/yyyy"),
            }).ToListAsync();

            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    var splitDurationToGetDays = item.Validity.Split(' ')[0];
                    int addDays = Convert.ToInt32(splitDurationToGetDays);

                    DateTime endDa = DateTime.ParseExact(item.StartDate, "dd/MM/yyyy", null);
                    endDa = endDa.AddDays(addDays);
                    if (endDa.Date < DateTime.Now.Date)
                    {
                        item.Status = "Expired";
                    }
                    else
                    {
                        item.Status = "Active";
                    }
                    item.EndDate = endDa.Date.ToString("dd/MM/yyyy");
                    if (item.Amount.ToLower() != "free")
                    {
                        item.Amount = "₦ " + Convert.ToDouble(item.Amount).ToString("N");
                    }
                    else
                    {
                        item.Amount = item.Amount;
                    }
                }
            }

            var userDevices = await _context.UserDevices.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserDevicesRecord
            {
                Type = x.Type,
                PurchaseDate = x.RegisteredDate.ToString("dd/MM/yyyy"),
                TypeId = x.TypeId,
                UserDeviceId = x.Id

            }).ToListAsync();
            if(userDevices.Count()>0)
            {
                foreach (var item in userDevices)
                {
                    if (item.Type.ToLower() == "modem")
                    {
                        var device = await _context.tblModem.Where(x => x.Id == item.TypeId).FirstOrDefaultAsync();
                        if (device != null)
                        {
                            item.Manufacturer = device.Manufacturer;

                            if (device.Amount.ToLower() != "free")
                            {
                                item.Amount = "₦ " + Convert.ToDouble(device.Amount).ToString("N");
                            }
                            else
                            {
                                item.Amount = device.Amount;
                            }
                        }
                        
                    }
                }
            }           

            var userDataSub = new UserDevicesSubscriptionsVM
            {
                UserSubscriptionList = data,
                UserDevicesList = userDevices
            };

            return userDataSub;
        }
        public async Task<RegisterNewVM> GetRegisterNew(string userId)
        {

            ////Load Programs from DB
            ////-------------------------------
            //List<Programs> programList = await _context.Programs.ToListAsync();
            //Load Programs from DB
            //-------------------------------
            List<Institutions> programList = await _context.Institutions.ToListAsync();

            programList.Insert(0, new Institutions { Id = 0, Name = "Select an option" });
            //Load data and modem
            //-------------------
            //var DMList = new List<DataModem>();
            var Data = await _context.tblData.Select(x=> new GetDataModem 
            {
                Id=x.Id,
                Amount = x.Amount,
                Bundle =x.Bundle,
                Duration =x.Duration,
                NetworkProvider =x.NetworkProvider

            }).ToListAsync();

            if(Data.Count()>0)
            {
                foreach(var item in Data)
                {
                    if(item.Amount.ToLower() == "free")
                    {
                        item.Amount = "(" + item.Amount +")";
                    }
                    else
                    {
                        item.Amount = "(₦ " + Convert.ToDouble(item.Amount).ToString("N") + ")";
                    }
                }
            }


            var Modem = await _context.tblModem.Select(x => new GetDataModem
            {
                Id = x.Id,
                Amount = x.Amount,
                Bundle = x.Bundle,
                //Duration = x.Duration,
                NetworkProvider = x.Manufacturer

            }).ToListAsync();

            if (Modem.Count() > 0)
            {
                foreach (var item in Modem)
                {
                    if (item.Amount.ToLower() == "free")
                    {
                        item.Amount = "(" + item.Amount + ")";
                    }
                    else
                    {
                        item.Amount = "(₦ " + Convert.ToDouble(item.Amount).ToString("N") + ")";
                    }

                    if (!item.NetworkProvider.Contains("Modem"))
                    {
                        item.NetworkProvider = item.NetworkProvider + " Modem";
                    }
                }
            }
            var perc = 0;
            var referCodeUsed = await _context.UserReferred.Include(x => x.Referral).Where(x => x.ReferredUserId == userId).Select(x => x.Referral.ReferralCode).FirstOrDefaultAsync();
            if(referCodeUsed == null || referCodeUsed == "")
            {
                var discount = await _context.UserDiscount.Where(x => x.ReferralId == userId).FirstOrDefaultAsync();
                if(discount != null)
                {
                    referCodeUsed = discount.Code;
                    perc = Convert.ToInt32(discount.Rate);
                }
            }
            //var certType = await _context.CertificateType.ToListAsync();
            var viewModel = new RegisterNewVM
            {
                programListz = programList,
                DataList = Data,
                ModemList = Modem,
                refCode = referCodeUsed,
                percentageOffer = perc

                //CertificateType = certType
            };

            return viewModel;
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
        public async Task<List<UserRequest>> GetRequest(string userId)
        {
            try
            {
               
                var userR = await _context.UserRequest.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).ToListAsync();
                return userR;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<UserRequest> SingleRequest(int Id, string userId)
        {
            try
            {

                var userR = await _context.UserRequest.Where(x => x.UserId == userId && x.Id == Id).FirstOrDefaultAsync();
                return userR;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<List<UserRequest>> SendRequest(UserRequest dto, string userId)
        {
            try
            {
                var req = new UserRequest
                {
                    Request = dto.Request,
                    RequestDate =DateTime.Now.Date,
                    Responded =false,
                    UserId = userId,
                    Title =dto.Title
                };

                await _context.UserRequest.AddAsync(req);
                await _context.SaveChangesAsync();

                var userR = await _context.UserRequest.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).ToListAsync();
                return userR;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public string ZeroPadUp(string value, int maxPadding, string prefix = null)
        {
            string result = value.PadLeft(maxPadding, '0');
            if (!string.IsNullOrEmpty(prefix)) { return prefix + result; }
            return result;
        }
        public async Task<bool> CreateWallet(UserWallet createWallet)
        {
            _context.UserWallet.Add(createWallet);
            await _context.SaveChangesAsync();
            return true;
            
        }
        public async Task<Response<Users>> Enrollment(RegisterUserDTO dto, string url)
        {
            try
            {
                
                var studentRoleId = await _context.AllUserRoles.Where(x => x.Name.ToLower() == "student").Select(x => x.Id).FirstOrDefaultAsync();
                if (dto.accountType.ToLower() == "personal")
                {
                    var usernameExist = await _userManager.Users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    if(usernameExist != null)
                    {
                        return new Response<Users>() { Data = usernameExist , Message ="Email exist", Successful =false};
                    }
                    var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    if (currentuser != null)
                    {
                        return new Response<Users>() { Data = usernameExist, Message = "Phone number exist.", Successful = false };

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
                        RoleId = studentRoleId,
                        RegisteredDate =DateTime.Now,
                        StaffDep = StaffDepEnums.None
                    };

                    var createdUser = await _userManager.CreateAsync(user, dto.personalReg.Password);

                    if (createdUser.Succeeded)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, dto.personalReg.Password,
                                                                    true, lockoutOnFailure: false);
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
                        await CreateWallet(new UserWallet
                        {
                            WalletId = ZeroPadUp(newWalletId.ToString(), 5, "3500") + 0,
                            CurrencyId = 1,
                            UserId =user.Id,
                            AvailableBalance = 0.0m,
                            SavingBalance = 0.0m,
                            EscrowBalance = 0.0m,
                            CreatedDate=DateTime.Now,
                            
                        });

                        //Populate UserReferral table
                        //---------------------------
                        if(dto.personalReg.referralCode != null && dto.personalReg.referralCode !="")
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
                       


                        return new Response<Users>() { Data = user, Message = "Successful", Successful = true };
                    }


                    return new Response<Users>() { Data = null, Message = "Not Successful", Successful = false };
                }
                else if (dto.accountType.ToLower() == "nysc")
                {
                    var usernameExist = await _userManager.Users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    if (usernameExist != null)
                    {
                        return new Response<Users>() { Data = usernameExist, Message = "Email exist", Successful = false };
                    }
                    var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    if (currentuser != null)
                    {
                        return new Response<Users>() { Data = usernameExist, Message = "Phone number exist.", Successful = false };

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
                        RoleId = studentRoleId,
                        RegisteredDate = DateTime.Now,
                        NYSC =true
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
                        await CreateWallet(new UserWallet
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
                        if(AnyNYSCPromo != null)
                        {

                        }

                        return new Response<Users>() { Data = user, Message = "Successful", Successful = true };
                    }


                    return new Response<Users>() { Data = null, Message = "Not Successful", Successful = false };
                }
                else
                {
                    var usernameExist = await _userManager.Users.Where(x => x.Email == dto.personalReg.Email).FirstOrDefaultAsync();
                    if (usernameExist != null)
                    {
                        return new Response<Users>() { Data = usernameExist, Message = "Email exist", Successful = false };
                    }
                    var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.personalReg.PhoneNo);
                    if (currentuser != null)
                    {
                        return new Response<Users>() { Data = usernameExist, Message = "Phone number exist.", Successful = false };

                    }

                    var user = new Users
                    {
                        FirstName = dto.businessReg.FirstName,
                        LastName = dto.businessReg.LastName,
                        MiddleName = dto.businessReg.MiddleName,
                        CityId = dto.businessReg.CityId,
                        Email = dto.businessReg.Email.ToLower(),
                        Address = dto.businessReg.Address,
                        Gender = dto.businessReg.Gender,
                        AlternatePhone = dto.businessReg.AlternatePhone,
                        PhoneNumber = dto.businessReg.PhoneNo,
                        UserName = dto.businessReg.Username.ToLower(),
                        StudentNumber = GenerateStudentId(),
                        Status = UserStatusEnums.Active,
                        RoleId = studentRoleId,
                        RegisteredDate = DateTime.Now
                    };

                    var createdUser = await _userManager.CreateAsync(user, dto.businessReg.Password);

                    if (createdUser.Succeeded)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, dto.businessReg.Password,
                                                                    true, lockoutOnFailure: false);
                        var business = new Businesses
                        {
                            Address = dto.businessReg.BusinessAddress,
                            Email = dto.businessReg.BusinessEmail,
                            Name = dto.businessReg.BusinessName,
                            Phone = dto.businessReg.BusinessPhone,
                            CityId = dto.businessReg.BusinessCityId
                        };

                        await _context.Businesses.AddAsync(business);
                        
                       if(await _context.SaveChangesAsync() >0)
                        {
                            var businessUser = new BusinessesUsers
                            {
                                UserRole =dto.businessReg.UserRole,
                                BusinessId= business.Id,
                                UserId = user.Id
                            };

                            await _context.BusinessesUsers.AddAsync(businessUser);
                            await _context.SaveChangesAsync();

                            //Create acccount wallet
                            int maxWalletId = await _context.UserWallet.MaxAsync(x => x.Id); ;
                            int newWalletId = maxWalletId + 1;

                            //Currency Id will be changed to NGN id once the whole currency is available in the db
                            await CreateWallet(new UserWallet
                            {
                                WalletId = ZeroPadUp(newWalletId.ToString(), 5, "3500") + 0,
                                CurrencyId = 1,
                                UserId = user.Id,
                                AvailableBalance = 0.0m,
                                SavingBalance = 0.0m,
                                EscrowBalance = 0.0m,
                                CreatedDate = DateTime.Now,

                            });
                        }
                        return new Response<Users>() { Data = user, Message = "Successful", Successful = true };
                    }

                    return new Response<Users>() { Data = null, Message = "Not Successful", Successful = false };
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
                if(user != null)
                {
                    var courseExist = await _context.UserCourses.Where(x => x.UserId == user.Id && x.CoursePriceOptionId == dto.userCourseOption.CoursePriceOptionId).FirstOrDefaultAsync();
                    if(courseExist != null)
                    {
                        return new PaymentInitialiseResponse() { errorMessage = "Course already exist, kindly login to your portal." };
                    }
                    var subRef = "";
                    PaymentMethodEnums paymentM = new PaymentMethodEnums();
                    if (dto.UserPayment.PaymentMethod.ToLower() == "card")
                    {
                        paymentM = PaymentMethodEnums.Card;
                        subRef = $"{DateTime.Now.Ticks}{(int)paymentM}";
                    }
                    else if (dto.UserPayment.PaymentMethod.ToLower() == "offline")
                    {
                        paymentM = PaymentMethodEnums.Offline;
                        subRef = dto.UserPayment.OfflinePaymentRef;
                    }
                    else if (dto.UserPayment.PaymentMethod.ToLower() == "interswitch")
                    {
                        paymentM = PaymentMethodEnums.InterSwitch;
                        subRef = $"{DateTime.Now.Ticks}{(int)paymentM}";
                    }

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
                        //if(dto.userCourseOption.CertificateTypeId == 0)
                        //{
                        //    certTypeId = await _context.CertificateType.Where(x => x.CertType == "Electronic").Select(x => x.Id).FirstOrDefaultAsync();
                        //}
                        //else
                        //{
                        //    certTypeId = dto.userCourseOption.CertificateTypeId;
                        //}

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
                        string discription = "";
                        if (dto.refCode != null && dto.refCode !="")
                        {               

                            //Get if the person is a new user
                            //-------------------------------

                            var referral = await _context.UserReferred.Include(x=>x.Referral).Where(x => x.ReferredUserId == dto.userId).FirstOrDefaultAsync();

                            if(referral != null)
                            {
                                discription = "Referral";
                                if ((UserRolesEnums)referral.Referral.RoleId == UserRolesEnums.Staff)
                                {
                                    discount = (7 * courseDate.coursePrice) / 100;
                                   // perAmount = dto.userCourseOption.AmountPaid;
                                    referral.ReferralDiscount = 3;
                                    referral.ReferredDiscount = 7;
                                }
                                else if ((UserRolesEnums)referral.Referral.RoleId == UserRolesEnums.Freelance)
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
                                    Amount=dto.userCourseOption.AmountPaid,
                                    UserCourseId = newCourseOp.Id
                                };

                                await _context.UserReferralPaymentHistory.AddAsync(refePH);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                //Discount Usage
                                //--------------
                               var disco = await _context.UserDiscount.Where(x => x.Code == dto.refCode && x.ReferralId == dto.userId).FirstOrDefaultAsync();
                                if(disco != null)
                                {
                                    discription = "Discount";

                                    if (disco.TotalApproved > disco.TotalApplied)
                                    {
                                        discount = (disco.Rate * courseDate.coursePrice) / 100;

                                        disco.TotalApplied += 1; 
                                        _context.UserDiscount.Update(disco);

                                        await _context.SaveChangesAsync();
                                    }
                                    
                                }
                            }
                        }

                        var pDTO = new PaymentDTO2()
                        {
                            Amount = (decimal)dto.userCourseOption.AmountPaid,
                            PaymentFor = paymentForEnums.Course,
                            UserPaymentForId = newCourseOp.Id,
                            UserId = dto.userId,
                            paymentRef = subRef,
                            Description= discription,
                           // perAmount = perAmount,
                            discountAm =discount
                        };

                        paymentList.Add(pDTO);

                        

                        if (dto.userCourseOption.CertificateTypeId != null && dto.userCourseOption.CertificateTypeId > 0)
                        {
                            var certType = await _context.CertificateType.Where(x => x.Id == dto.userCourseOption.CertificateTypeId).FirstOrDefaultAsync();
                            if (certType.CertType.ToLower() == "physical")
                            {
                                var ctDTO = new PaymentDTO2()
                                {
                                    Amount = (decimal)certType.Fee,
                                    PaymentFor = paymentForEnums.PhysicalCertificate,
                                    UserPaymentForId = newCourseOp.Id,
                                    UserId = dto.userId,
                                    paymentRef = subRef
                                };
                                paymentList.Add(ctDTO);
                            }
                        }

                        if (dto.userCourseOption.deliveryStateId != null && dto.userCourseOption.deliveryStateId > 0)
                        {
                            var courierFee = await _context.States.Where(x => x.Id == dto.userCourseOption.deliveryStateId).Select(x => x.CourierFee).FirstOrDefaultAsync();
                            var dsDTO = new PaymentDTO2()
                            {
                                Amount = (decimal)courierFee,
                                PaymentFor = paymentForEnums.Courier,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = subRef
                            };
                            paymentList.Add(dsDTO);
                        }

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
                        var rateAmount = await GetCertificationConvertedValuebyCertOptId(dto.userCertificationOption.CertificationPriceOptionId);
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

                    if (dto.userDataOption != null && dto.userDataOption.DataId > 0)
                    {
                        var newCourseOp = new UserData
                        {
                            DataId = dto.userDataOption.DataId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                        };

                        await _context.UserData.AddAsync(newCourseOp);

                        await _context.SaveChangesAsync();
                        decimal dataFee = 0;
                        var strdataFee = await _context.tblData.Where(x => x.Id == dto.userDataOption.DataId).Select(x => x.Amount).FirstOrDefaultAsync();
                        if (strdataFee.ToLower() != "free")
                        {
                            dataFee = Convert.ToDecimal(strdataFee);

                            var pDTO = new PaymentDTO2()
                            {
                                Amount = dataFee,
                                PaymentFor = paymentForEnums.Data,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = subRef
                            };
                            paymentList.Add(pDTO);
                        }

                    }

                    if (dto.userModemOption != null && dto.userModemOption.ModemId > 0)
                    {
                        var newCourseOp = new UserDevices
                        {
                            Type = "Modem",
                            TypeId = dto.userModemOption.ModemId,
                            RegisteredDate = DateTime.Now,
                            UserId = dto.userId,
                        };

                        await _context.UserDevices.AddAsync(newCourseOp);
                        await _context.SaveChangesAsync();

                        decimal modemFee = 0;
                        var strmodemFee = await _context.tblModem.Where(x => x.Id == dto.userModemOption.ModemId).Select(x => x.Amount).FirstOrDefaultAsync();
                        if (strmodemFee.ToLower() != "free")
                        {
                            modemFee = Convert.ToDecimal(strmodemFee);

                            var pDTO = new PaymentDTO2()
                            {
                                Amount = modemFee,
                                PaymentFor = paymentForEnums.Modem,
                                UserPaymentForId = newCourseOp.Id,
                                UserId = dto.userId,
                                paymentRef = subRef
                            };
                            paymentList.Add(pDTO);
                        }

                    }
                    
                    switch (paymentM)
                    {
                        case PaymentMethodEnums.Card:
                            var cardResponse = await InitializeCardPayment2(paymentList, paymentM, "Dashboard");
                            return cardResponse;
                        case PaymentMethodEnums.Offline:
                            var OfflineResponse = await InitializeOfflinePayment2(paymentList, "Dashboard");
                            return OfflineResponse;
                        case PaymentMethodEnums.InterSwitch:
                            var InterSwitchResponse = await InitializeInterSwitchPayment2(paymentList);
                            return InterSwitchResponse;
                    }
                    throw new Exception("No subscription method found");

                }
                return new PaymentInitialiseResponse() { errorMessage = "Invalid user details." };

            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<PaymentInitialiseResponse> InitializeCardPayment2(List<PaymentDTO2> dto, PaymentMethodEnums paymentMethod, string redirectURL)
        {
            try
            {
                //var paymentMList = new List<PaymentMethodEnums>();

                //paymentMList.Add(paymentMethod);
                //paymentMList.Add(PaymentMethodEnums.AccountTransfer);

                using (var httpClientHandler = new HttpClientHandler())
                {
                    using (var _apiClient = new HttpClient(httpClientHandler))
                    {     
                        var userId = "";
                        var subRef = "";
                        decimal totalAmountPaid =0;
                        decimal changedCourseAmountPaid =0;
                        UserPaymentHistory paymentHistroyDis = new UserPaymentHistory();
                        foreach (var item in dto)
                        {
                            subRef = item.paymentRef;
                            float amount = 0;
                            if (item.UserOldPaymentForId > 0)
                            {                                   
                                var oldPaymentCourse = await _context.UserPaymentHistory.Where(x => x.UserId == item.UserId && x.UserPaymentForId == item.UserOldPaymentForId).ToListAsync();
                                if (oldPaymentCourse.Count()>0)
                                {
                                    var courseOnly = oldPaymentCourse.Where(x => x.PaymentFor == paymentForEnums.Course).ToList();
                                    var othersOnly = oldPaymentCourse.Where(x => x.PaymentFor != paymentForEnums.Course).ToList();
                                    foreach (var rec in courseOnly)
                                    {
                                        rec.ChangedToUserPaymentForId = item.UserPaymentForId;
                                        rec.CourseOptionDateChanged = true;
                                        _context.UserPaymentHistory.Update(rec);
                                    }

                                    foreach (var rec1 in othersOnly)
                                    {
                                        rec1.UserPaymentForId = item.UserPaymentForId;
                                        _context.UserPaymentHistory.Update(rec1);
                                    }

                                    await _context.SaveChangesAsync();
                                }
                                amount = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == item.UserPaymentForId).Select(x => x.CoursePriceOption.Amount).FirstOrDefaultAsync();
                               
                            }
                            else
                            {
                                amount = (float)item.Amount;
                            }

                            userId = item.UserId;
                            totalAmountPaid += item.Amount;
                            var Description = "";
                            var PaymentS = new PaymentStatusEnums();
                            var PaymentM = new PaymentMethodEnums();
                            if (item.PaymentFor == paymentForEnums.Promo)
                            {
                                Description = "Promo Code Payment";
                                PaymentS = PaymentStatusEnums.Initialized;
                                PaymentM = PaymentMethodEnums.Promo;
                            }
                            else
                            {
                                Description = "Card Payment";
                                PaymentS = PaymentStatusEnums.Initialized;
                                PaymentM = PaymentMethodEnums.Card;
                            }
                            paymentHistroyDis = new UserPaymentHistory
                            {
                                Amount = (decimal)amount,
                                PaymentRef = item.paymentRef,
                                Description = Description,
                                StatusId = PaymentS,
                                PaymentMethodId = PaymentM,
                                PaymentDate = DateTime.Now,
                                PaymentFor = item.PaymentFor,
                                UserId =item.UserId,
                                UserPaymentForId = item.UserPaymentForId
                            };
                            _context.UserPaymentHistory.Add(paymentHistroyDis);

                            //Saving the percentage offered by using referral code
                            //----------------------------------------------------
                            if (item.discountAm != null && item.discountAm > 0)
                            {
                                //Check if usercourse Exist
                                //-------------------------
                                if(item.Description == "Referral")
                                {
                                    var existUC = await _context.UserPaymentHistory.Where(x => x.UserPaymentForId == item.UserPaymentForId && x.UserId == userId).FirstOrDefaultAsync();
                                    if (existUC == null)
                                    {
                                        decimal discountAm = (decimal)item.discountAm;

                                        paymentHistroyDis = new UserPaymentHistory
                                        {
                                            Amount = discountAm,
                                            PaymentRef = item.paymentRef,
                                            Description = "Referral Code Usage",
                                            StatusId = PaymentS,
                                            PaymentMethodId = PaymentMethodEnums.Referral,
                                            PaymentDate = DateTime.Now,
                                            PaymentFor = item.PaymentFor,
                                            UserId = item.UserId,
                                            UserPaymentForId = item.UserPaymentForId
                                        };
                                        _context.UserPaymentHistory.Add(paymentHistroyDis);
                                    }
                                }
                                else if (item.Description == "Discount")
                                {
                                    var existUC = await _context.UserPaymentHistory.Where(x => x.UserPaymentForId == item.UserPaymentForId && x.UserId == userId).FirstOrDefaultAsync();
                                    if (existUC == null)
                                    {
                                        decimal discountAm = (decimal)item.discountAm;

                                        paymentHistroyDis = new UserPaymentHistory
                                        {
                                            Amount = discountAm,
                                            PaymentRef = item.paymentRef,
                                            Description = "Discount Usage",
                                            StatusId = PaymentS,
                                            PaymentMethodId = PaymentMethodEnums.Discount,
                                            PaymentDate = DateTime.Now,
                                            PaymentFor = item.PaymentFor,
                                            UserId = item.UserId,
                                            UserPaymentForId = item.UserPaymentForId
                                        };
                                        _context.UserPaymentHistory.Add(paymentHistroyDis);
                                    }
                                }
                            }

                        }
                        
                        await _context.SaveChangesAsync();

                        
                        string[] paymentMethods = { paymentMethod.ToDescription().Replace(" ", "_").ToUpper(), PaymentMethodEnums.AccountTransfer.ToDescription().Replace(" ", "_").ToUpper() };
                  
                        var user = await _userManager.FindByIdAsync(userId);

                        var subAccountsDeta = new List<IncomeSplitConfig>();

                        ////Sub Account for Test Mode
                        ////-------------------------
                        //var subAcct1 = new IncomeSplitConfig
                        //{
                        //    subAccountCode = "MFY_SUB_435879694665",
                        //    splitAmount = 20
                        //};
                        //subAccountsDeta.Add(subAcct1);
                        //var subAcct2 = new IncomeSplitConfig
                        //{
                        //    subAccountCode = "MFY_SUB_325932989523",
                        //    splitAmount = 30
                        //};
                        //subAccountsDeta.Add(subAcct2);

                        ////End of Sub Accounts for Test Mode
                        ////---------------------------------

                        //Sub Account for Live Mode
                        //-------------------------
                        var subAcct1 = new IncomeSplitConfig
                        {
                            subAccountCode = "MFY_SUB_244000011484",
                            splitAmount = (20 / 100) * totalAmountPaid
                        };
                        subAccountsDeta.Add(subAcct1);
                        var subAcct2 = new IncomeSplitConfig
                        {
                            subAccountCode = "MFY_SUB_015431140047",
                            splitAmount = (30 / 100) * totalAmountPaid
                        };
                        subAccountsDeta.Add(subAcct2);
                        var subAcct3 = new IncomeSplitConfig
                        {
                            subAccountCode = "MFY_SUB_050350086439",
                            splitAmount = (10 / 100) * totalAmountPaid
                        };
                        subAccountsDeta.Add(subAcct3);
                        var subAcct4 = new IncomeSplitConfig
                        {
                            subAccountCode = "MFY_SUB_898054811442",
                            splitAmount = (10 / 100) * totalAmountPaid
                        };
                        subAccountsDeta.Add(subAcct4);
                        var subAcct5 = new IncomeSplitConfig
                        {
                            subAccountCode = "MFY_SUB_400293597473",
                            splitAmount = (30 / 100) * totalAmountPaid
                        };
                        subAccountsDeta.Add(subAcct5);
                        //End of Sub Accounts for Live Mode
                        //---------------------------------


                        MonnifyPaymentInitializationRequest request = new MonnifyPaymentInitializationRequest
                        {
                            Amount = totalAmountPaid,
                            CustomerName = $"{user.LastName } {user.FirstName}",
                            CustomerEmail = user.Email,
                            PaymentReference = subRef,
                            PaymentDescription = "Payment",
                            CurrencyCode = "NGN",
                            ContractCode = _apiSettings.ContractCode,
                            RedirectUrl = $"{_apiSettings.PaymentRedirectUrl}/{redirectURL}?paymentRef={subRef}",
                            PaymentMethods = paymentMethods,
                            IncomeSplitConfig = subAccountsDeta
                        };

                        var authenticationString = $"{_apiSettings.MonnifyKey}:{_apiSettings.MonnifySecret}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

                        var requestLogin = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/api/v1/auth/login");
                        requestLogin.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
                        var content = new StringContent("", null, "application/json");
                        requestLogin.Content = content;
                        var responseLogin = await _apiClient.SendAsync(requestLogin);
                        if (responseLogin.IsSuccessStatusCode)
                        {
                            var result = await responseLogin.Content.ReadAsStringAsync();
                            LoginResponse re = JsonConvert.DeserializeObject<LoginResponse>(result);

                            var token = re.responseBody.accessToken;

                            var requestLoad = JsonConvert.SerializeObject(request);
                            var requestPayment = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/api/v1/merchant/transactions/init-transaction");
                            requestPayment.Headers.Add("Authorization", "Bearer " + token);
                            var PaymentContent = new StringContent(JsonConvert.SerializeObject(request), null, "application/json");
                            //var content2 = new StringContent("", null, "application/json");
                            requestPayment.Content = PaymentContent;
                            var responsePayment = await _apiClient.SendAsync(requestPayment);

                            var jsonInitResponse = await responsePayment.Content.ReadAsStringAsync();
                            MonnifyPaymentInitializationResponse initDeserializeResponse = JsonConvert.DeserializeObject<MonnifyPaymentInitializationResponse>(jsonInitResponse);

                            if (initDeserializeResponse.RequestSuccessful)
                            {
                                var allPaymentHistory = await _context.UserPaymentHistory.Where(x => x.PaymentRef == subRef).ToListAsync();
                                if (allPaymentHistory.Count() > 0)
                                {
                                    foreach (var item in allPaymentHistory)
                                    {
                                        item.Description = initDeserializeResponse.ResponseDetails.TransactionReference;
                                        item.StatusId = PaymentStatusEnums.Pending;
                                    }
                                }
                                await _context.SaveChangesAsync();

                                var response = new PaymentInitialiseResponse()
                                {
                                    checkOutURL = initDeserializeResponse.ResponseDetails.CheckoutUrl,
                                    paymentRef = subRef
                                };
                                return response;

                            }
                            var err = new PaymentInitialiseResponse()
                            {
                                checkOutURL = "Error Occured",
                                paymentRef = subRef
                            };
                            return err;
                        }                     




                        
                        //var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/api/v1/auth/login");
                        //requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic ", base64EncodedAuthenticationString);
                        //requestMessage.Content = initContent;
                        //var initResponse = await _apiClient.SendAsync(requestMessage);
                        
                        return null;
                    }
                }
            }
            catch (Exception Ex)
            {
                var mes = Ex.Message;
                throw Ex;
            }
        }
        public async Task<PaymentInitialiseResponse> InitializeOfflinePayment2(List<PaymentDTO2> dto, string redirectURL)
        {
            try
            {
                //$"{DateTime.Now.Ticks}{(int)PaymentMethodEnums.Offline}";
                var subRef = "";
                UserPaymentHistory paymentHistroyDis = new UserPaymentHistory();
                foreach (var item in dto)
                {
                    //userId = item.UserId;
                    //totalAmountPaid += item.Amount;
                    float amount = 0;
                    if (item.UserOldPaymentForId > 0)
                    {
                        var oldPaymentCourse = await _context.UserPaymentHistory.Where(x => x.UserId == item.UserId && x.UserPaymentForId == item.UserOldPaymentForId).ToListAsync();
                        if (oldPaymentCourse.Count() > 0)
                        {
                            var courseOnly = oldPaymentCourse.Where(x => x.PaymentFor == paymentForEnums.Course).ToList();
                            var othersOnly = oldPaymentCourse.Where(x => x.PaymentFor != paymentForEnums.Course).ToList();
                            foreach (var rec in courseOnly)
                            {
                                rec.ChangedToUserPaymentForId = item.UserPaymentForId;
                                rec.CourseOptionDateChanged = true;
                                _context.UserPaymentHistory.Update(rec);
                            }

                            foreach (var rec1 in othersOnly)
                            {
                                rec1.UserPaymentForId = item.UserPaymentForId;
                                _context.UserPaymentHistory.Update(rec1);
                            }

                            await _context.SaveChangesAsync();
                        }
                        amount = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == item.UserPaymentForId).Select(x => x.CoursePriceOption.Amount).FirstOrDefaultAsync();

                    }
                    else
                    {
                        amount = (float)item.Amount;
                    }
                    
                    subRef = item.paymentRef;
                    var Description = "";
                    var PaymentS = new PaymentStatusEnums();
                    var PaymentM = new PaymentMethodEnums();
                    if (item.PaymentFor == paymentForEnums.Promo)
                    {
                        Description = "Promo Code Payment";
                        PaymentS = PaymentStatusEnums.Pending;
                        PaymentM = PaymentMethodEnums.Promo;
                    }
                    else 
                    {
                        Description = "Offline Payment";
                        PaymentS = PaymentStatusEnums.Pending;
                        PaymentM = PaymentMethodEnums.Offline;
                    }
                    paymentHistroyDis = new UserPaymentHistory
                    {
                        Amount = (decimal)amount,
                        PaymentRef = item.paymentRef,
                        Description = Description,
                        StatusId = PaymentS,
                        PaymentMethodId = PaymentM,
                        PaymentDate = DateTime.Now,
                        PaymentFor = item.PaymentFor,
                        UserId = item.UserId,
                        UserPaymentForId = item.UserPaymentForId
                    };
                    _context.UserPaymentHistory.Add(paymentHistroyDis);

                    //Saving the percentage offered by using referral code
                    //----------------------------------------------------
                    if (item.discountAm!= null &&  item.discountAm > 0)
                    {
                        //Check if usercourse Exist
                        //-------------------------
                        if (item.Description == "Referral")
                        {
                            var existUC = await _context.UserPaymentHistory.Where(x => x.UserPaymentForId == item.UserPaymentForId && x.UserId == item.UserId).FirstOrDefaultAsync();
                            if (existUC == null)
                            {
                                decimal discountAm = (decimal)item.discountAm;

                                paymentHistroyDis = new UserPaymentHistory
                                {
                                    Amount = discountAm,
                                    PaymentRef = item.paymentRef,
                                    Description = "Referral Code Usage",
                                    StatusId = PaymentS,
                                    PaymentMethodId = PaymentMethodEnums.Referral,
                                    PaymentDate = DateTime.Now,
                                    PaymentFor = item.PaymentFor,
                                    UserId = item.UserId,
                                    UserPaymentForId = item.UserPaymentForId
                                };
                                _context.UserPaymentHistory.Add(paymentHistroyDis);
                            }
                        }
                        else if (item.Description == "Discount")
                        {
                            var existUC = await _context.UserPaymentHistory.Where(x => x.UserPaymentForId == item.UserPaymentForId && x.UserId == item.UserId).FirstOrDefaultAsync();
                            if (existUC == null)
                            {
                                decimal discountAm = (decimal)item.discountAm;

                                paymentHistroyDis = new UserPaymentHistory
                                {
                                    Amount = discountAm,
                                    PaymentRef = item.paymentRef,
                                    Description = "Discount Usage",
                                    StatusId = PaymentS,
                                    PaymentMethodId = PaymentMethodEnums.Discount,
                                    PaymentDate = DateTime.Now,
                                    PaymentFor = item.PaymentFor,
                                    UserId = item.UserId,
                                    UserPaymentForId = item.UserPaymentForId
                                };
                                _context.UserPaymentHistory.Add(paymentHistroyDis);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                var response = new PaymentInitialiseResponse()
                {
                    checkOutURL = "Successful",
                    paymentRef = subRef
                };
                return response;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
        public async Task<PaymentInitialiseResponse> InitializeInterSwitchPayment2(List<PaymentDTO2> dto)
        {
            try
            {
                //$"{DateTime.Now.Ticks}{(int)PaymentMethodEnums.Offline}";
                var subRef = "";
                UserPaymentHistory paymentHistroyDis = new UserPaymentHistory();
                foreach (var item in dto)
                {
                    //userId = item.UserId;
                    //totalAmountPaid += item.Amount;
                    float amount = 0;
                    if (item.UserOldPaymentForId > 0)
                    {
                        var oldPaymentCourse = await _context.UserPaymentHistory.Where(x => x.UserId == item.UserId && x.UserPaymentForId == item.UserOldPaymentForId).ToListAsync();
                        if (oldPaymentCourse.Count() > 0)
                        {
                            var courseOnly = oldPaymentCourse.Where(x => x.PaymentFor == paymentForEnums.Course).ToList();
                            var othersOnly = oldPaymentCourse.Where(x => x.PaymentFor != paymentForEnums.Course).ToList();
                            foreach (var rec in courseOnly)
                            {
                                rec.ChangedToUserPaymentForId = item.UserPaymentForId;
                                rec.CourseOptionDateChanged = true;
                                _context.UserPaymentHistory.Update(rec);
                            }

                            foreach (var rec1 in othersOnly)
                            {
                                rec1.UserPaymentForId = item.UserPaymentForId;
                                _context.UserPaymentHistory.Update(rec1);
                            }

                            await _context.SaveChangesAsync();
                        }
                        amount = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == item.UserPaymentForId).Select(x => x.CoursePriceOption.Amount).FirstOrDefaultAsync();

                    }
                    else
                    {
                        amount = (float)item.Amount;
                    }

                    subRef = item.paymentRef;
                    paymentHistroyDis = new UserPaymentHistory
                    {
                        Amount = (decimal)amount,
                        PaymentRef = item.paymentRef,
                        Description = "InterSwitch",
                        StatusId = PaymentStatusEnums.Initialized,
                        PaymentMethodId = PaymentMethodEnums.Card,
                        PaymentDate = DateTime.Now,
                        PaymentFor = item.PaymentFor,
                        UserId = item.UserId,
                        UserPaymentForId = item.UserPaymentForId
                    };
                    _context.UserPaymentHistory.Add(paymentHistroyDis);
                }

                await _context.SaveChangesAsync();
                var response = new PaymentInitialiseResponse()
                {
                    checkOutURL = "Successful",
                    paymentRef = subRef
                };
                return response;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public async Task<Register1VM> GetRegister()
        {
            //Load Countries from DB
            //----------------------
            var country = await _context.Countries.ToListAsync();

            //Load Nigeria States from DB
            //---------------------------
            var states = await _context.States.Where(x=>x.CountryId == 163).ToListAsync();
            
            var viewModel = new Register1VM
            {
                countryListz = country,
                nigeriaStatesListz = states
            };

            return viewModel;
        }
        public async Task<List<Programs>> GetPrograms()
        {
            //Load program by program category Id from DB
            //-------------------------------------------
            var programs = await _context.Programs.ToListAsync();
            return programs;
        }
        public async Task<List<CertificateType>> GetCertificateType()
        {
            //Load program by program category Id from DB
            //-------------------------------------------
            var programs = await _context.CertificateType.ToListAsync();
            return programs;
        }
        public async Task<string> GetCertificateFeeByTypeId(int TypeId)
        {
            //Load program by program category Id from DB
            //-------------------------------------------
            var result = "";
            var fee = await _context.CertificateType.Where(x=>x.Id== TypeId).Select(x=>x.Fee).FirstOrDefaultAsync();
            if(fee != null && fee >0)
            {
                result = "₦ " + fee.ToString("N");
            }
            return result;
        }
        public async Task<List<ProgramOptions>> GetProgramOptionsByCategoryId(int CategoryId)
        {
            //Load program options by program Id from DB
            //-------------------------------------------
            var programs = await _context.ProgramOptions.Where(x => x.CategoryId == CategoryId).ToListAsync();
            return programs;
        }
        public async Task<List<Subjects>> GetProgramSubjectssByOptionId(int OptionId)
        {
            //Load program subjects by program Id from DB
            //-------------------------------------------
            var programs = await _context.Subjects.Where(x => x.ProgramOptionId == OptionId).ToListAsync();
            return programs;
        }
        public async Task<List<Countries>> GetCountries()
        {
            try
            {
                var countries = await _context.Countries.ToListAsync();

                return countries;
            }
            catch (Exception ex)
            {
                var ms = ex.Message;
                throw ex;
            }
            
        }
        public async Task<List<States>> GetStatesByCountryId(int CountryId)
        {
            //Load States by countryId from DB
            //--------------------------------
            var state = await _context.States.Include(x=>x.Country).Where(x=>x.CountryId== CountryId).ToListAsync();
            
            return state;
        }
        public async Task<List<Cities>> GetCitiesByStateId(int StateId)
        {
            var cities = await _context.Cities.Include(x => x.State).ThenInclude(x => x.Country).Where(x => x.StateId == StateId).ToListAsync();
            return cities;
            ////Load Cities by stateId from LocationAPI
            ////---------------------------------------
            //var city = await _addressServices.GetAllCityByStateId(StateId);
            //var allCities = new List<Cities>();
            //foreach (var item in city.data.cities)
            //{
            //    var cityList = new Cities()
            //    {
            //        cityId = item.cityId,
            //        cityName = item.cityName
            //    };

            //    allCities.Add(cityList);
            //}
            //return allCities;
        }
        public async Task<string> GetCourierFeeByStateId(int StateId)
        {
            var result = "";
            var courierFee = await _context.States.Where(x => x.Id == StateId).Select(x=>x.CourierFee).FirstOrDefaultAsync();
            if (courierFee != null && courierFee > 0)
            {
                result = "₦ " + courierFee.ToString("N");
            }
            return result;           
        }
        public async Task<string> GetAddressByCityId(int CityId)
        {
            //Load States by countryId from DB
            //--------------------------------
            var city = await _context.Cities.Include(x => x.State).ThenInclude(x=>x.Country).Where(x=>x.Id == CityId).FirstOrDefaultAsync();
            string result = city.State.Country.Name + "," + city.State.Name + "," + city.Name;
            return result;
        }

        //public async Task<List<Streets>> GetStreetsByCityId(int CityId)
        //{
        //    //Load Cities by stateId from LocationAPI
        //    //---------------------------------------
        //    var street = await _addressServices.GetAllStreetByCityId(CityId);
        //    var allStreets = new List<Streets>();
        //    if(street.data != null)
        //    {
        //        foreach (var item in street.data.streetName)
        //        {
        //            var streetList = new Streets()
        //            {
        //                streetId = item.streetId,
        //                streetName = item.streetName
        //            };

        //            allStreets.Add(streetList);
        //        }
        //    }            
        //    return allStreets;
        //}

        public async Task<List<Institutions>> GetInstitutions()
        {
            //Load Institutions from DB
            //---------------------
            var institutionList = await _context.Institutions.ToListAsync();

            return institutionList;
        }
        public async Task<List<Courses>> GetCoursesbyProgramCat(int ProgramCatId)
        {
            //Load Subjects from DB
            //---------------------
            var courseList = await _context.Courses.Where(x=>x.CategoryId == ProgramCatId).ToListAsync();

            return courseList;
        }
        public async Task<List<CourseOptionDates>> GetCourseOptionsDatesbyCourseId(int CourseId)
        {
            try 
            {
                //var courseList = await _context.CoursePriceOptions.Include(x=>x.Course).ThenInclude(x=>x.Category).ThenInclude(x=>x.Institution).Where(x => x.CourseId == CourseId && (x.StartDate.Year >= DateTime.Now.Year) && (x.StartDate.Month >= DateTime.Now.Month)).OrderByDescending(x=>x.Id).Take(2).Select(x => new CourseOptionDates
                //{
                //    //Id = x.Id,
                //    DateRange =  $"{x.StartDate.Date.ToString("MMMM yyyy")} To {x.EndDate.Date.ToString("MMMM yyyy")}",
                //    institutionName = x.Course.Category.Institution.Name

                //}).Distinct().ToListAsync();
                var result = new List<CourseOptionDates>();
                var courseList = await _context.CoursePriceOptions.Include(x => x.Course).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.CourseId == CourseId && (x.StartDate.Year >= DateTime.Now.Year)).OrderByDescending(x => x.Id).Select(x => new { startdate = x.StartDate, enddate = x.EndDate }).Distinct().Take(2).ToListAsync();//.Take(2).Select(x=> new { startdate = x.StartDate, enddate = x.EndDate }).ToListAsync();

                if (courseList.Count > 0)
                {
                    foreach (var item in courseList)
                    {
                        var eachResult = new CourseOptionDates();

                        if (item.startdate.Year == DateTime.Now.Year)
                        {
                            if (item.startdate.Month >= DateTime.Now.Month)
                            {
                                eachResult.DateRange = $"{item.startdate.Date.ToString("MMMM yyyy")} To {item.enddate.Date.ToString("MMMM yyyy")}";

                                result.Add(eachResult);
                            }
                        }
                        else
                        {
                            eachResult.DateRange = $"{item.startdate.Date.ToString("MMMM yyyy")} To {item.enddate.Date.ToString("MMMM yyyy")}";

                            result.Add(eachResult);
                        }
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                throw ex;
            }
            
        }
        public async Task<List<CourseOptionDetails>> GetCourseOptionsbyOptionDate(string OptionDate, int selectedCourseId)
        {
            try
            {
                DateTime startDate = new DateTime();
                DateTime endDate = new DateTime();
                if (OptionDate != null && OptionDate != "")
                {
                    startDate = Convert.ToDateTime(OptionDate.Trim().Split("To")[0]);
                    endDate = Convert.ToDateTime(OptionDate.Trim().Split("To")[1]);
                }
                var courseLis = await _context.CoursePriceOptions.Where(x => x.CourseId == selectedCourseId && (x.StartDate.Month == startDate.Month && x.StartDate.Year == startDate.Year)).Select(x => new // && (x.EndDate.Month == endDate.Month && x.EndDate.Month == endDate.Month)).Select(x => new 
                {
                    Id = x.Id,
                    DateRange = $"{x.StartDate.Date.ToString("MMMM yyyy")} To {x.EndDate.Date.ToString("MMMM yyyy")}",
                    Name = $"{x.Name} - {x.Duration} - ₦ {x.Amount.ToString("N")}"                   

                }).ToListAsync();

                courseLis = courseLis.Distinct().ToList();

                var courseList = new List<CourseOptionDetails>();
                if (courseLis.Count()>0)
                {
                    foreach (var item in courseLis)
                    {
                        if(item.DateRange == OptionDate)
                        {
                            var eachCO = new CourseOptionDetails
                            {
                                Id = item.Id,
                                Name = item.Name
                            };

                            courseList.Add(eachCO);
                        }
                        
                    }
                }

                return courseList;
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                throw ex;
            }

        }
        public async Task<List<Certifications>> GetCertificatesbyCourseId(int CourseId)
        {
            var courseList =await (from c in _context.Courses
                              join ca in _context.ProgramCategory on c.CategoryId equals ca.Id
                              join cert in _context.Certifications on ca.Id equals cert.CategoryId

                              where c.Id == CourseId
                              select new Certifications { Id = cert.Id, Name = cert.Name }
                              ).ToListAsync();

            return courseList;
        }
        public async Task<List<Certifications>> GetCertificatesbyCategoryId(int CategoryId)
        {
            var courseList = await _context.Certifications.Where(x=>x.CategoryId == CategoryId).ToListAsync();

            return courseList;
        }
        public async Task<List<CertificateOptionDates>> GetCertificatesOptionsbyId(int CertId)
        {
            try
            {                
                var result = await _context.CertificationPriceOptions.Include(x=>x.Currency).Where(x => x.Id == CertId).ToListAsync();

                var courseList = new List<CertificateOptionDates>();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {                       
                        if (item.Currency.name == "₦")
                        {
                            var eachCO = new CertificateOptionDates
                            {
                                Id = item.Id,
                                ExamDate =$"{item.ExamDate.ToString("dd/MM/yyyy")} - {item.Currency.major_symbol} {Convert.ToDouble(item.Amount).ToString("N")}"
                            };
                            courseList.Add(eachCO);
                        }
                        else
                        {
                            var eachCO = new CertificateOptionDates
                            {
                                Id = item.Id,
                                ExamDate = $"{item.ExamDate.ToString("dd/MM/yyyy")} - {item.Currency.major_symbol} {item.Amount}"
                            };
                            courseList.Add(eachCO);

                        }

                    }
                }
               
                return courseList;
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                throw ex;
            }

        }
        public async Task<string> GetCertificationConvertedValuebyCertOptId(int CertOptId)
        {
            try
            {
                var result = await (from cc in _context.CurrencyConversion
                                    join c in _context.Currency on cc.CurrencyId equals c.Id
                                    join certOpt in _context.CertificationPriceOptions on c.Id equals certOpt.CurrencyId
                                    where certOpt.Id == CertOptId
                                    select new {amount = certOpt.Amount, rate = cc.ConversionRate, charges = certOpt.Charges, currency = certOpt.Currency.major_symbol }).FirstOrDefaultAsync();
                string convertedValue = "";
                if (result != null)
                {
                    convertedValue = result.amount +","+ result.rate + "," + result.charges + "," +result.currency;
                }
                return convertedValue;
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                throw ex;
            }

        }
        //public async Task<string> GetPriceByProgramId(int ProgramId)
        //{
        //    //Load Program Price by programId from DB
        //    //---------------------------------------
        //    var price = await _context.Programs.Where(x=>x.Id == ProgramId).Select(x=> new { price = x.PriceNGN, deposit = x.DepositNGN, duration = x.Duration}).FirstOrDefaultAsync();

        //    var result = "";
        //    if(price != null)
        //    {
        //        result = price.price.ToString("N") + "=" + price.deposit.ToString("N") + "=" + price.duration + "="+ price.price + "=" + price.deposit;
        //    }
        //    return result;
        //}
        public async Task<string> GetPriceByProgramOptionId(int ProgramOptionId)
        {
            //Load Program option Price by programOptionId from DB
            //----------------------------------------------------
            var price = await _context.ProgramOptions.Where(x => x.Id == ProgramOptionId).Select(x => new { price = x.PriceNGN, deposit = x.DepositNGN, duration = x.Duration, maxSubjects = x.MaxSubjectSelection, USD = x.PriceUSD }).FirstOrDefaultAsync();

            var result = "";
            if (price != null)
            {
                result = price.price.ToString("N") + "=" + price.deposit.ToString("N") + "=" + price.duration + "=" + price.price + "=" + price.deposit + "="+ price.maxSubjects + "=" + price.USD;
            }
            return result;
        }
        public async Task<string> QueryBankCOnnectPayment(string PaymentRef)
        {
            int paymentType = Convert.ToInt32(PaymentRef.Substring(PaymentRef.Length - 1));
            if(paymentType == 5)
            {
                return "Successful";
            }

            int payRefCount = PaymentRef.Length;// PaymentRef.Substring(PaymentRef.Length - 1);
            //paymentType = int.Parse(payRef);
            var result = "";
            if(payRefCount <= 10)
            {
                result = await FinalizeMonocoPayment(PaymentRef);
            }
            else
            {
                result = await FinalizeMonnifyPayment(PaymentRef);
            }
            return result;
        }
        public async Task<string> FinalizeMonnifyPayment(string paymentReference)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                using (var _apiClient = new HttpClient(httpClientHandler))
                {
                    //Get the transaction details
                    var PaymentDetails = await _context.UserPaymentHistory.Include (x=>x.User).Where(x => x.PaymentRef == paymentReference && x.Description != "Discount").FirstOrDefaultAsync();
                    if (PaymentDetails == null)
                    {
                        return $"Payment Reference {paymentReference} not found";
                    }
                    else if(PaymentDetails.StatusId == PaymentStatusEnums.Paid)
                    {
                        GeneralClass.pRef = "";
                        return "Updated";
                    }
                    //Get the access token
                    var authenticationString = $"{_apiSettings.MonnifyKey}:{_apiSettings.MonnifySecret}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
                    var authRequest = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/api/v1/auth/login");
                    authRequest.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
                    var content = new StringContent("", null, "application/json");
                    authRequest.Content = content;
                    var authResponse = await _apiClient.SendAsync(authRequest);
                    if (authResponse.IsSuccessStatusCode)
                    {
                        var jsonAuthResponse = await authResponse.Content.ReadAsStringAsync();
                        LoginResponse re = JsonConvert.DeserializeObject<LoginResponse>(jsonAuthResponse);

                        var token = re.responseBody.accessToken;

                        if (!re.requestSuccessful)
                        {
                            throw new Exception($"Status verification access failed. {re.responseMessage}");
                        }

                        //Call the status verification endpoint
                        string encodedTransRef = System.Web.HttpUtility.UrlEncode(PaymentDetails.Description);
                        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_apiSettings.MonnifyBaseUrl}/api/v2/transactions/{encodedTransRef}");
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        var statusResponse = await _apiClient.SendAsync(requestMessage);
                        var jsonStatusResponse = await statusResponse.Content.ReadAsStringAsync();
                        MonnifyPaymentStatusResponse deserializedStatusResponse = JsonConvert.DeserializeObject<MonnifyPaymentStatusResponse>(jsonStatusResponse);


                        //var programTotalAmount = await _context.ProgramOptions.Where(x => x.Id == PaymentDetails.UserProgramOption.ProgramOptionId).Select(x => new { programAmount = x.PriceNGN, startDate = x.StartDate }).FirstOrDefaultAsync();
                        //var userProgram = await _context.UserProgramOption.Where(x => x.UserId == PaymentDetails.UserProgramOption.UserId && x.ProgramOptionId == PaymentDetails.UserProgramOption.ProgramOptionId).FirstOrDefaultAsync();
                        //Update the transaction table and invoice table if the payment is successful
                        if (deserializedStatusResponse.RequestSuccessful)
                        {
                            if (deserializedStatusResponse.ResponseDetails.PaymentStatus.Equals("PAID"))
                            {
                                //Update the user's total available subscription
                                //----------------------------------------------
                                if (PaymentDetails.StatusId != PaymentStatusEnums.Paid)
                                {
                                    var allPayment = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentRef == paymentReference && x.Description != "Discount").ToListAsync();
                                    var coursePayment = new UserPaymentHistory();
                                    var promoPayment = new UserPaymentHistory();
                                    var certPayment = new UserPaymentHistory();
                                    var dataPayment = new UserPaymentHistory();
                                    var modemPayment = new UserPaymentHistory();
                                    foreach (var item in allPayment)
                                    {
                                        item.StatusId = PaymentStatusEnums.Paid;

                                        if (item.PaymentMethodId != PaymentMethodEnums.Referral && item.PaymentMethodId != PaymentMethodEnums.Discount)
                                        {
                                            if (deserializedStatusResponse.ResponseDetails.PaymentMethod.ToLower() == "card")
                                            {
                                                item.PaymentMethodId = PaymentMethodEnums.Card;
                                            }
                                            else if (deserializedStatusResponse.ResponseDetails.PaymentMethod.ToLower() == "accounttransfer")
                                            {
                                                item.PaymentMethodId = PaymentMethodEnums.AccountTransfer;
                                            }
                                        }                                          
                                        
                                        //item.PaymentMethodId = deserializedStatusResponse.ResponseDetails.PaymentMethod;
                                        _context.UserPaymentHistory.Update(item);

                                        if (item.PaymentFor == paymentForEnums.Course)
                                        {
                                            coursePayment = item;
                                        }
                                        else if (item.PaymentFor == paymentForEnums.Certifications)
                                        {
                                            certPayment = item;
                                        }
                                        else if (item.PaymentFor == paymentForEnums.Data)
                                        {
                                            dataPayment = item;
                                        }
                                        else if (item.PaymentFor == paymentForEnums.Modem)
                                        {
                                            modemPayment = item;
                                        }
                                        else if (item.PaymentFor == paymentForEnums.Promo)
                                        {
                                            promoPayment = item;

                                            var promoUsage = new PromoUsageHistory
                                            {
                                                PromoId = promoPayment.UserPaymentForId,
                                                UserId = promoPayment.UserId
                                            };

                                            await _context.PromoUsageHistory.AddAsync(promoUsage);
                                            await _context.SaveChangesAsync();
                                        }
                                    }

                                    await _context.SaveChangesAsync();

                                    //Get user course payment history
                                    //------------------------------
                                    var totalCourseAmountPaid = await _context.UserPaymentHistory.Where(x => x.UserId == PaymentDetails.UserId && x.UserPaymentForId == coursePayment.UserPaymentForId && x.PaymentFor == paymentForEnums.Course && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();
                                    var Coursepromo = await _context.UserPaymentHistory.Where(x => x.UserId == PaymentDetails.UserId && x.UserPaymentForId == promoPayment.UserPaymentForId && x.PaymentFor == paymentForEnums.Promo && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                                    totalCourseAmountPaid += Coursepromo;

                                    //Get program total amount
                                    //------------------------

                                    float programTotalAmount = 0;
                                    var det = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == coursePayment.UserPaymentForId).Select(x => new { amount = x.CoursePriceOption.Amount, courseId = x.CoursePriceOption.CourseId, Id = x.Id }).FirstOrDefaultAsync();
                                    if (det != null)
                                    {
                                        programTotalAmount = det.amount;
                                    }

                                    if ((float)totalCourseAmountPaid < programTotalAmount)
                                    {
                                        var changedCourseAmount = await _context.UserPaymentHistory.Where(x => x.UserId == PaymentDetails.UserId && x.CourseOptionDateChanged == true && x.ChangedToUserPaymentForId == coursePayment.UserPaymentForId).SumAsync(x => x.Amount);
                                        var tA = totalCourseAmountPaid + changedCourseAmount;
                                        if ((float)tA < programTotalAmount)
                                        {
                                            var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                                            userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;
                                            _context.UserCourses.Update(userCourse);
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                                            userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                                            _context.UserCourses.Update(userCourse);
                                            await _context.SaveChangesAsync();
                                            //return "Successful";                                    
                                        }
                                        //return "Successful";
                                    }
                                    else
                                    {
                                        var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.Id == det.Id).FirstOrDefaultAsync();
                                        userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                                        _context.UserCourses.Update(userCourse);
                                        await _context.SaveChangesAsync();
                                        //return "Successful";                                    
                                    }

                                    var cert = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Where(x => x.UserId == PaymentDetails.UserId && x.Id == certPayment.UserPaymentForId).FirstOrDefaultAsync();
                                    if (cert != null)
                                    {
                                        cert.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                                        _context.UserCertifications.Update(cert);

                                    }

                                    var data = await _context.UserData.Include(x => x.Data).Where(x => x.UserId == PaymentDetails.UserId && x.Id == dataPayment.UserPaymentForId).FirstOrDefaultAsync();
                                    if (data != null)
                                    {
                                        data.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                                        _context.UserData.Update(data);

                                    }

                                    var modem = await _context.UserDevices.Where(x => x.UserId == PaymentDetails.UserId && x.Id == modemPayment.UserPaymentForId).FirstOrDefaultAsync();
                                    if (modem != null)
                                    {
                                        modem.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                                        _context.UserDevices.Update(modem);

                                    }

                                    //Manage Referral Code
                                    //--------------------
                                    var referralCodeUsed = await _context.UserReferred.Where(x => x.ReferredUserId == PaymentDetails.UserId).FirstOrDefaultAsync();
                                    if(referralCodeUsed != null)
                                    {
                                        var ActualReferredUserDis = await _context.UserPaymentHistory.Where(x => x.UserId == PaymentDetails.UserId && x.UserPaymentForId == det.Id && x.PaymentMethodId == PaymentMethodEnums.Referral).Select(x=>x.Amount).FirstOrDefaultAsync();
                                        decimal amountLeftToPay =(decimal)programTotalAmount - ActualReferredUserDis;

                                        var CourseAmtPaid = await _context.UserPaymentHistory.Where(x => x.UserId == PaymentDetails.UserId && x.UserPaymentForId == det.Id && x.PaymentMethodId != PaymentMethodEnums.Referral && x.PaymentRef == paymentReference).Select(x => x.Amount).FirstOrDefaultAsync();
                                        var percPaidOfamountLeftToPay = (CourseAmtPaid * 100) / amountLeftToPay;

                                        var ActualReferralDis = (referralCodeUsed.ReferralDiscount * programTotalAmount)/100;

                                        var EachReferralEarning = (percPaidOfamountLeftToPay * Convert.ToInt32(ActualReferralDis)) / 100;

                                        var userRPH = await _context.UserReferralPaymentHistory.Include(x=>x.UserRefer).Where(x => x.PaymentRef == paymentReference && x.UserCourseId == det.Id && x.UserRefer.ReferredUserId == PaymentDetails.UserId).FirstOrDefaultAsync();
                                        if(userRPH != null)
                                        {
                                            userRPH.Earning = (float)EachReferralEarning;

                                            _context.UserReferralPaymentHistory.Update(userRPH);
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                    //if (item.PaymentMethodId == PaymentMethodEnums.Referral)
                                    //{
                                    //    var userRefPHDetails = await _context.UserReferralPaymentHistory.Include(x => x.UserRefer).ThenInclude(x => x.Referral).Where(x => x.UserRefer.ReferredUserId == PaymentDetails.UserId && x.PaymentRef == paymentReference).FirstOrDefaultAsync();
                                    //    if (userRefPHDetails != null)
                                    //    {
                                    //        decimal referralPercentage = 0;
                                    //        if ((UserRolesEnums)userRefPHDetails.UserRefer.Referral.RoleId == UserRolesEnums.Staff)
                                    //        {
                                    //            var actualCourseP = (item.Amount * 100) / 7;
                                    //            var amountDis = (decimal)userRefPHDetails.Amount + item.Amount;

                                    //            if (actualCourseP)
                                    //                referralPercentage = (decimal)(3 * userRefPHDetails.Amount) / 100;
                                    //        }
                                    //        else if ((UserRolesEnums)userRefPHDetails.UserRefer.Referral.RoleId == UserRolesEnums.Freelance)
                                    //        {
                                    //            var actualCourseP = (item.Amount * 100) / 5;

                                    //            referralPercentage = (decimal)(5 * userRefPHDetails.Amount) / 100;
                                    //        }
                                    //        userRefPHDetails.Earning = (float)referralPercentage;

                                    //        _context.UserReferralPaymentHistory.Update(userRefPHDetails);
                                    //        await _context.SaveChangesAsync();
                                    //    }
                                    //}
                                    //else
                                    //{

                                    //}
                                    await _context.SaveChangesAsync();
                                    return "Successful";
                                    //var refCode = await _context.UserReferred.Include(x=>x.Referral).Where(x =>x.PaymentRef == paymentReference).FirstOrDefaultAsync();// _context.UserPaymentHistory.Where(x => x.UserProgramOptionId == PaymentDetails.UserProgramOptionId).OrderBy(x => x.Id).Select(x => x.ReferralDiscountCode).FirstOrDefaultAsync();
                                    //if(refCode != null)
                                    //{
                                    //    decimal earni = 0;
                                    //    //if(refCode.Referral.Role == UserRolesEnums.Student)
                                    //    //{
                                    //    //    earni = (PaymentDetails.Amount * 15/2) / 100;
                                    //    //}
                                    //    //else if (refCode.Referral.Role == UserRolesEnums.Marketing)
                                    //    //{
                                    //    //    earni = (PaymentDetails.Amount * 10) / 100;
                                    //    //}
                                    //    ////else if (refCode.Referral.Role == UserRolesEnums.Enrollment)
                                    //    ////{
                                    //    ////    earni = (PaymentDetails.Amount * (15/2)) / 100;
                                    //    ////}

                                    //    refCode.Earnings =(float)earni;

                                    //    _context.UserReferred.Update(refCode);

                                    //}

                                }

                            }
                            //if (programTotalAmount.startDate.Date > DateTime.Now.Date)
                            //{
                            //    userProgram.ProgramStatus = UserProgramStatusEnums.Pending;
                            //}
                            //else if (programTotalAmount.startDate.Date == DateTime.Now.Date)
                            //{
                            //    userProgram.ProgramStatus = UserProgramStatusEnums.InProgress;
                            //}
                            //else
                            //{
                            //    userProgram.ProgramStatus = UserProgramStatusEnums.Completed;
                            //}
                            ////userProgram.ProgramStatus = UserProgramStatusEnums.InProgress;
                            //userProgram.RegDate = DateTime.Now.Date;
                            //_context.UserProgramOption.Update(userProgram);
                            //await _context.SaveChangesAsync();
                            //return "Successful";
                        }


                    }



                    //var authRequest = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/v1/auth/login");
                    //authRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    //var authResponse = await _apiClient.SendAsync(authRequest);
                    //var jsonAuthResponse = await authResponse.Content.ReadAsStringAsync();
                    //MonnifyAuthenticationResponse deserializedAuthResponse = JsonConvert.DeserializeObject<MonnifyAuthenticationResponse>(jsonAuthResponse);


                    return "";
                }
            }
            //return "";
        }
        public async Task<string> FinalizeMonocoPayment(string PaymentRef)
        {
            //Get the transaction details

            var PaymentDetails = await _context.UserPaymentHistory.Include(x => x.User).Where(x => x.PaymentRef == PaymentRef && x.Description != "Discount").FirstOrDefaultAsync();
            if (PaymentDetails == null)
            {
                return $"Payment Reference {PaymentRef} not found";
            }
            //Get program total amount
            //------------------------
            //var programTotalAmount = await _context.ProgramOptions.Where(x => x.Id == PaymentDetails.UserProgramOption.ProgramOptionId).Select(x => new { programAmount = x.PriceNGN, startDate = x.StartDate }).FirstOrDefaultAsync();
            //var userProgram = await _context.UserProgramOption.Where(x => x.UserId == PaymentDetails.UserProgramOption.UserId && x.ProgramOptionId == PaymentDetails.UserProgramOption.ProgramOptionId).FirstOrDefaultAsync();

            var client = new RestClient($"{_apiSettings.MonocoBaseUrl }/v1/payments");
            var request = new RestRequest("verify", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("mono-sec-key", $"{_apiSettings.MonocoSecretKey }");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\"reference\":\"" + PaymentRef + "\"}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            MonocoPaymentStatusResponse initDeserializeResponse = JsonConvert.DeserializeObject<MonocoPaymentStatusResponse>(response.Content);
            decimal amountPaid = 0;

            if (initDeserializeResponse.data.Status == "successful")
            {
                amountPaid = decimal.Parse(initDeserializeResponse.data.Amount) / 100;

                PaymentDetails.StatusId = PaymentStatusEnums.Paid;

                _context.UserPaymentHistory.Update(PaymentDetails);
                await _context.SaveChangesAsync();

                //Get user course payment history
                //------------------------------
                var totalAmountPaid = await _context.UserPaymentHistory.Where(x => x.UserPaymentForId == PaymentDetails.UserPaymentForId && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                //Get program total amount
                //------------------------

                float programTotalAmount = 0;

                if (PaymentDetails.PaymentFor == paymentForEnums.Course)
                {
                    var det = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CoursePriceOption.Amount, courseId = x.CoursePriceOption.CourseId }).FirstOrDefaultAsync();
                    // var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.CourseId == det.courseId).FirstOrDefaultAsync();
                    programTotalAmount = det.amount;
                }
                else if (PaymentDetails.PaymentFor == paymentForEnums.Certifications)
                {
                    var det = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CertificationPriceOption.Amount, certId = x.CertificationPriceOption.CertificationId }).FirstOrDefaultAsync();
                    //var userCert = await _context.UserCertifications.Include(x => x.User).Include(x => x.Certification).Where(x => x.UserId == PaymentDetails.UserId && x.CertificationId == det.certId).FirstOrDefaultAsync();
                    programTotalAmount = det.amount;
                }
                if ((float)totalAmountPaid < programTotalAmount)
                {
                    //userProgram.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;
                    if (PaymentDetails.PaymentFor == paymentForEnums.Course)
                    {
                        var det = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CoursePriceOption.Amount, courseId = x.CoursePriceOption.CourseId }).FirstOrDefaultAsync();
                        var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.CoursePriceOption.CourseId == det.courseId).FirstOrDefaultAsync();
                        userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;


                        //userProgram.ProgramStatus = UserProgramStatusEnums.InProgress;
                        userCourse.RegisteredDate = DateTime.Now.Date;
                        _context.UserCourses.Update(userCourse);
                        await _context.SaveChangesAsync();
                        return "Successful";
                    }
                    else if (PaymentDetails.PaymentFor == paymentForEnums.Certifications)
                    {
                        var det = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CertificationPriceOption.Amount, certId = x.CertificationPriceOption.CertificationId }).FirstOrDefaultAsync();
                        var userCert = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).Where(x => x.UserId == PaymentDetails.UserId && x.CertificationPriceOption.CertificationId == det.certId).FirstOrDefaultAsync();
                        userCert.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;

                        userCert.RegisteredDate = DateTime.Now.Date;
                        _context.UserCertifications.Update(userCert);
                        await _context.SaveChangesAsync();
                        return "Successful";
                    }
                }
                else
                {
                    if (PaymentDetails.PaymentFor == paymentForEnums.Course)
                    {
                        var det = await _context.UserCourses.Include(x => x.CoursePriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CoursePriceOption.Amount, courseId = x.CoursePriceOption.CourseId }).FirstOrDefaultAsync();
                        var userCourse = await _context.UserCourses.Include(x => x.User).Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == PaymentDetails.UserId && x.CoursePriceOption.CourseId == det.courseId).FirstOrDefaultAsync();
                        userCourse.PaymentStatus = UserProgramPaymentStatusEnums.Paid;

                        userCourse.RegisteredDate = DateTime.Now.Date;
                        _context.UserCourses.Update(userCourse);
                        await _context.SaveChangesAsync();
                        return "Successful";
                    }
                    else if (PaymentDetails.PaymentFor == paymentForEnums.Certifications)
                    {
                        var det = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Where(x => x.Id == PaymentDetails.UserPaymentForId).Select(x => new { amount = x.CertificationPriceOption.Amount, certId = x.CertificationPriceOption.CertificationId }).FirstOrDefaultAsync();
                        var userCert = await _context.UserCertifications.Include(x => x.CertificationPriceOption).Include(x => x.User).Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).Where(x => x.UserId == PaymentDetails.UserId && x.CertificationPriceOption.CertificationId == det.certId).FirstOrDefaultAsync();
                        userCert.PaymentStatus = UserProgramPaymentStatusEnums.Paid;

                        userCert.RegisteredDate = DateTime.Now.Date;
                        _context.UserCertifications.Update(userCert);
                        await _context.SaveChangesAsync();
                        return "Successful";
                    }
                }
                //Get user payment history
                //------------------------
                //var totalAmountPaid = await _context.UserPaymentHistory.Where(x => x.UserProgramOptionId == PaymentDetails.UserProgramOptionId && x.StatusId == PaymentStatusEnums.Paid).Select(x => x.Amount).SumAsync();

                //if ((float)totalAmountPaid < programTotalAmount.programAmount)
                //{
                //    userProgram.PaymentStatus = UserProgramPaymentStatusEnums.Deposited;
                //}
                //else
                //{
                //    userProgram.PaymentStatus = UserProgramPaymentStatusEnums.Paid;
                //}

                //var refCode = await _context.UserReferred.Include(x => x.Referral).Where(x => x.PaymentRef == PaymentRef).FirstOrDefaultAsync();// _context.UserPaymentHistory.Where(x => x.UserProgramOptionId == PaymentDetails.UserProgramOptionId).OrderBy(x => x.Id).Select(x => x.ReferralDiscountCode).FirstOrDefaultAsync();
                //if (refCode != null)
                //{
                //    decimal earni = 0;
                //    //if (refCode.Referral.Role == UserRolesEnums.Student)
                //    //{
                //    //    earni = (PaymentDetails.Amount * 15/2) / 100;
                //    //}
                //    //else if (refCode.Referral.Role == UserRolesEnums.Marketing)
                //    //{
                //    //    earni = (PaymentDetails.Amount * 10) / 100;
                //    //}
                //    ////else if (refCode.Referral.Role == UserRolesEnums.Enrollment)
                //    ////{
                //    ////    earni = (PaymentDetails.Amount * (15 / 2)) / 100;
                //    ////}

                //    refCode.Earnings = (float)earni;

                //    _context.UserReferred.Update(refCode);

                //}
            }
            else
            {
                return "Not Successful";
            }
            //if (programTotalAmount.startDate.Date > DateTime.Now.Date)
            //{
            //    userProgram.ProgramStatus = UserProgramStatusEnums.Pending;
            //}
            //else if (programTotalAmount.startDate.Date == DateTime.Now.Date)
            //{
            //    userProgram.ProgramStatus = UserProgramStatusEnums.InProgress;
            //}
            //else
            //{
            //    userProgram.ProgramStatus = UserProgramStatusEnums.Completed;
            //}
            ////userProgram.ProgramStatus = UserProgramStatusEnums.InProgress;
            //userProgram.RegDate = DateTime.Now.Date;
            //_context.UserProgramOption.Update(userProgram);
            //await _context.SaveChangesAsync();

            return "Successful";
        }
        public async Task<PaymentInitialiseResponse> RegisterUser(RegisterDTO dto)
        {
            
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);


            if (existingUser != null)
            {
                return new PaymentInitialiseResponse() { errorMessage = "Email already exist." };
            }
            else
            {
                var currentuser = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.Phone);
                if(currentuser != null)
                {
                    return new PaymentInitialiseResponse() { errorMessage = "Phone number already exist." };

                }

                //var superLo = await _addressServices.CreateLocation(dto.CityId, dto.StreetName, dto.StreetNumber);

                //var SuperId = superLo.data.superId;
                //var lastUser = await _context.users.OrderByDescending(x=>x.UserId).Select(x=>x.UserId).FirstOrDefaultAsync();
                //int count = 0;
                //if (lastUser != null && lastUser !="")
                //{
                //    count = Convert.ToInt32(lastUser);
                //}
                
               // count = count + 1;
                var rCode = GenerateReferalCode();
                var newUser = new Users
                {
                    Email = dto.Email.ToLower(),
                    UserName = dto.Email.ToLower(),
                    PhoneNumber = dto.Phone,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Gender = dto.Gender,
                    //StateId = dto.StateId,
                    CityId = dto.CityId,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    EmailConfirmed = true,
                    MiddleName = dto.MiddleName,
                    Status = UserStatusEnums.Active,
                    ReferralCode = rCode,
                    RegisteredDate = DateTime.Now.Date,
                    AlternatePhone =dto.AlternatePhone,
                    StudentNumber = "",//$"{count:0000}",
                    heardAboutUs =dto.heardAboutUs,
                   // Role = UserRolesEnums.Student
                };

                var createdUser = await _userManager.CreateAsync(newUser, dto.Password);

                if(createdUser.Succeeded)
                {
                    //Add user program into DB
                    //-------------------------
                    var userPro = new UserProgramOption()
                    {
                        ProgramOptionId = dto.ProgramOptionId,
                        UserId = newUser.Id,
                        RegDate = DateTime.Now
                    };
                    await _context.UserProgramOption.AddAsync(userPro);

                    //Add user subjects into DB
                    //-------------------------                    
                    if (dto.SubjectIds !=null && dto.SubjectIds.Count() > 0)
                    {
                        var userSub = new UserSubjects()
                        {
                            SubjectIds = string.Join(",", dto.SubjectIds),
                            UserId = newUser.Id
                        };
                        await _context.UserSubjects.AddAsync(userSub);
                    }

                    if (dto.Choices.FirstInstitution > 0)
                    {
                        //Add user first choice into DB
                        //-------------------------
                        var userFChoi = new UserChoices()
                        {
                            ProgramId = dto.ProgramOptionId,
                            InstitutionId = dto.Choices.FirstInstitution,
                            CourseId = dto.Choices.FirstCourse,
                            UserId = newUser.Id
                        };
                        await _context.UserChoices.AddAsync(userFChoi);
                    }

                    
                    if (dto.Choices.SecondInstitution > 0)
                    {
                        
                        //Add user second choice into DB
                        //-------------------------
                        var userSChoi = new UserChoices()
                        {
                            ProgramId = dto.ProgramOptionId,
                            InstitutionId = dto.Choices.SecondInstitution,
                            CourseId = dto.Choices.SecondCourse,
                            UserId = newUser.Id
                        };
                        await _context.UserChoices.AddAsync(userSChoi);
                    }
                    await _context.SaveChangesAsync();
                                       

                    //Send Email
                    //----------
                    var builder = new StringBuilder();
                    await _smtpMail.SendEmail(dto.Email, "Edurex Academy registration message", "Edurex Onboarding Successful");

                    //Send SMS
                    //--------
                   // await _smtpMail.SendSMS(dto.Phone, "Edurex Onboarding Successful");

                    PaymentMethodEnums paymentMethod = (PaymentMethodEnums)dto.PaymentMethod;

                    //var depositValue = await _context.Programs.Where(x => x.Id == dto.ProgramId).Select(x => x.Deposit).FirstOrDefaultAsync();
                    var pDTO = new PaymentDTO()
                    {
                        Amount =(decimal)dto.Amount,
                        BankConnectPaymenttype = "onetime-debit",
                        PaymentMethod = paymentMethod,
                        userProgramOptionId = userPro.Id,
                        ReferralDiscountCode = dto.referralCode
                    };

                    switch (paymentMethod)
                    {
                        case PaymentMethodEnums.Card:
                            var cardResponse = await InitializeCardPayment(pDTO, paymentMethod,"Dashboard");
                            return cardResponse;//Ok(new Response { IsSuccessful = true, Message = StatusMessage.OK, Code = ResponseCode.OK, Data = cardResponse });

                        case PaymentMethodEnums.AccountTransfer:
                            var transferResponse = await InitializeAccountTransferPayment(pDTO, paymentMethod, "Dashboard");
                            return transferResponse;//Ok(new Response { IsSuccessful = true, Message = StatusMessage.OK, Code = ResponseCode.OK, Data = transferResponse });

                        case PaymentMethodEnums.BankConnect:
                            var BankConnectResponse = await InitializeBankCOnnectPayment(pDTO, "Dashboard");
                            return BankConnectResponse;// Ok(new Response { IsSuccessful = true, Message = StatusMessage.OK, Code = ResponseCode.OK, Data = BankConnectResponse });
                        case PaymentMethodEnums.Offline:
                            var OfflineResponse = await InitializeOfflinePayment(pDTO, "Dashboard");
                            return OfflineResponse;// Ok(new Response { IsSuccessful = true, Message = StatusMessage.OK, Code = ResponseCode.OK, Data = BankConnectResponse });

                    }
                    throw new Exception("No subscription method found");
                }
                return new PaymentInitialiseResponse() { errorMessage = "Error creating user." };
            }
        }
        public async Task<string> Logout()
        {

            await _signInManager.SignOutAsync();
            return "successful";
        }
        public async Task<Tuple<Users, string, string>> Login(LoginDTO dto)
        {

            var existingUser = await _userManager.FindByEmailAsync(dto.email);


            if (existingUser == null)
            {
                return new Tuple<Users, string, string>(null, "Not Found", "");
            }
            else
            {
                var result = await _signInManager.PasswordSignInAsync(existingUser, dto.password,true, lockoutOnFailure: false);
                if(result.Succeeded)
                {
                    var completedEnroll = await _context.UserCourses.Where(x => x.UserId == existingUser.Id).FirstOrDefaultAsync();
                    if (completedEnroll != null)
                    {
                        return new Tuple<Users, string, string>(existingUser, "Successful", "");

                    }
                    var referCodeUsed = await _context.UserReferred.Include(x => x.Referral).Where(x => x.ReferredUserId == existingUser.Id).Select(x => x.Referral.ReferralCode).FirstOrDefaultAsync();
                    if(referCodeUsed != null && referCodeUsed != "")
                    {
                        return new Tuple<Users, string, string>(existingUser, "Incomplete", referCodeUsed);
                    }

                    var userDiscountExist = await _context.UserDiscount.Where(x => x.ReferralId == existingUser.Id).FirstOrDefaultAsync();
                    if(userDiscountExist != null)
                    {
                        referCodeUsed = userDiscountExist.Code;
                        return new Tuple<Users, string, string>(existingUser, "Incomplete", referCodeUsed);
                    }
                    return new Tuple<Users, string, string>(existingUser, "Incomplete", "");
                    // Enum.GetName(typeof(UserRolesEnums), existingUser.Role) ;
                }
                return new Tuple<Users, string, string>(existingUser, "Cannot login", "");

            }
        }
        public async Task<StudentDashboardVM> DashboardRe(string userId, string IP4)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(userId);


                if (existingUser != null)
                {
                    var userCour = await _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).ThenInclude(x=>x.Category).ThenInclude(x=>x.Institution).Where(x => x.UserId == userId && x.Status != UserCourseStatusEnums.Canceled).OrderByDescending(x => x.Id).Select(x => new UserCourseRecord
                    {
                        //certificateType = x.CertificateType.CertType,
                        CourierState = x.deliveryStateId.ToString(),
                        CourseCode = x.CoursePriceOption.Course.CourseCode,
                        Name = x.CoursePriceOption.Course.Name,
                        Price = x.CoursePriceOption.Amount.ToString(),
                        ProgramPaymentStatus = x.PaymentStatus,
                        ProgramStatus = x.Status,
                        StartDate = x.CoursePriceOption.StartDate.Date.ToString("dd/MM/yyyy"),
                        userCourseId = x.Id,
                        InstitutionCode =x.CoursePriceOption.Course.Category.Institution.ShortName,
                        CourseId = x.CoursePriceOption.CourseId

                    }).Take(5).ToListAsync();

                    if (userCour.Count() > 0)
                    {
                        foreach (var item in userCour)
                        {
                            item.Name = item.Name.Length <= 20 ? item.Name : item.Name.Substring(0, 20) + "...";

                            var amountPaid = _context.UserPaymentHistory.Where(c => c.UserId == userId && c.PaymentFor == paymentForEnums.Course && c.UserPaymentForId == item.userCourseId && c.StatusId == PaymentStatusEnums.Paid).Select(c => c.Amount).Sum();

                            decimal owing = Convert.ToDecimal(item.Price) - amountPaid;
                            item.amountOwing = "₦ " + owing.ToString("N");

                            item.amountPaid = "₦ " + amountPaid.ToString("N");
                            item.Price = "₦ " + Convert.ToDouble(item.Price).ToString("N");
                            

                            if (item.CourierState != null && item.CourierState != "")
                            {
                                item.CourierState = await _context.States.Where(x => x.Id == Convert.ToInt32(item.CourierState)).Select(x => x.Name).FirstOrDefaultAsync();
                            }
                        }
                    }

                    var userCert = await _context.UserCertifications.Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserCertificationsRecord
                    {
                        CertificationMode = x.CertificationPriceOption.Certification.Mode,
                        ExamDate = x.CertificationPriceOption.ExamDate.Date.ToString("dd/MM/yyyy"),
                        Name = x.CertificationPriceOption.Certification.Name,
                        Price = x.CertificationPriceOption.Amount.ToString(),
                        currency = x.CertificationPriceOption.Currency.major_symbol,
                        ProgramStatus = x.Status,
                        ProgramPaymentStatus = x.PaymentStatus,
                        ShortCode = x.CertificationPriceOption.Certification.ShortCode,
                        userCertificationId = x.Id,
                        CertOptId = x.CertificationPriceOptionId,
                        provider =x.CertificationPriceOption.Certification.OrganisationName

                    }).Take(5).ToListAsync();

                    if (userCert.Count() > 0)
                    {
                        foreach (var item in userCert)
                        {
                            var rateAmount = await GetCertificationConvertedValuebyCertOptId(item.CertOptId);
                            var amount = rateAmount.Split(',')[0];
                            var rate = rateAmount.Split(',')[1];

                            if (item.currency != null && item.currency != "₦")
                            {
                                item.Price = "₦ " + (Convert.ToDouble(amount) * Convert.ToDouble(rate)).ToString("N");
                            }
                            else
                            {
                                item.Price = "₦ " + item.Price;
                            }
                        }
                    }

                    var data = await _context.UserData.Include(x => x.Data).Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserDataRecord
                    {
                        Amount = x.Data.Amount,
                        Bundle = x.Data.Bundle,
                        Validity = x.Data.Duration,
                        NetworkProvider = x.Data.NetworkProvider,
                        StartDate = x.RegisteredDate.Date.ToString("dd/MM/yyyy"),
                    }).Take(5).ToListAsync();

                    if (data.Count() > 0)
                    {
                        foreach(var item in data )
                        {
                            var splitDurationToGetDays = item.Validity.Split(' ')[0];
                            int addDays = Convert.ToInt32(splitDurationToGetDays);

                            DateTime endDa = DateTime.ParseExact(item.StartDate, "dd/MM/yyyy", null); 
                            endDa =endDa.AddDays(addDays);
                            if(endDa.Date < DateTime.Now.Date)
                            {
                                item.Status = "Expired";
                            }
                            else
                            {
                                item.Status = "Active";
                            }
                            item.EndDate = endDa.Date.ToString("dd/MM/yyyy");
                            if(item.Amount.ToLower() != "free")
                            {
                                item.Amount = "₦ " + Convert.ToDouble(item.Amount).ToString("N");
                            }
                            else
                            {
                                item.Amount = item.Amount;
                            }
                        }
                    }
                    //var modem = await _context.UserModem.Include(x => x.Modem).Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserDataModemRecord
                    //{
                    //    Amount = x.Modem.Amount,
                    //    Bundle = x.Modem.Bundle,
                    //    NetworkProvider = x.Modem.NetworkProvider,
                    //    StartDate = x.RegisteredDate.Date.ToString("dd/MM/yyyy")

                    //}).Take(5).ToListAsync();

                   
                    //if (modem.Count() > 0)
                    //{
                    //    userDataMod.AddRange(modem);
                    //}

                    var payment = await _context.UserPaymentHistory.Where(x => x.UserId == userId && x.Description != "Changed Date").OrderByDescending(x => x.Id).Select(x => new 
                    {
                        Amount = x.Amount,//"₦ " + x.Amount.ToString("N"),
                        PaymentDate = x.PaymentDate,
                        PaymentMethod = x.PaymentMethodId,
                        PaymentRef = x.PaymentRef,
                        Status = x.StatusId,
                        paymentfor = x.PaymentFor,
                        paymentForId = x.UserPaymentForId

                    }).ToListAsync();
                    var userPay = new List<PaymentRecord>();
                    if (payment.Count() > 0)
                    {
                        var eachTransId = payment.GroupBy(i => i.PaymentRef).Select(group => new
                        {
                            pRef = group.Key,
                            TotalAmount = group.Sum(g => g.Amount),
                            PaymentDate = group.Select(x=>x.PaymentDate),
                            PaymentMethod = group.Select(x => x.PaymentMethod),
                        });

                        foreach(var item in eachTransId)
                        {                           

                            var eachUserPay = new PaymentRecord
                            {
                                Amount = "₦ " + item.TotalAmount.ToString("N"),
                                PaymentDate= item.PaymentDate.FirstOrDefault(),
                                PaymentMethod = item.PaymentMethod.FirstOrDefault(),
                                PaymentRef =item.pRef
                            };

                            //Getting the payment status of course
                            //------------------------------------
                            foreach (var item1 in payment)
                            {
                                if(item1.PaymentRef == item.pRef && item1.paymentfor == paymentForEnums.Course)
                                {
                                    eachUserPay.Status = item1.Status;
                                }
                            }
                            userPay.Add(eachUserPay);
                        }

                       
                        //foreach (var item in payment)
                        //{                           
                        //    //var CourierStateId = 0;
                        //    if (item.paymentfor == paymentForEnums.Course)
                        //    {
                        //        if(item.Status != PaymentStatusEnums.Paid)
                        //        {
                        //            eachTransId
                        //        }
                        //        //var course = await _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.UserId == userId && x.Id == item.paymentForId).Select(x => new { courseCode = x.CoursePriceOption.Course.CourseCode, deliveryStateId = x.deliveryStateId }).FirstOrDefaultAsync();
                        //        //if (course != null)
                        //        //{
                        //        //    item.paymentForName = course.courseCode;
                        //        //    CourierStateId = course.deliveryStateId;
                        //        //}

                        //    }
                        //    //if (item.paymentfor == paymentForEnums.Certifications)
                        //    //{
                        //    //    item.paymentForName = await _context.UserCertifications.Include(x => x.CertificationPriceOption).ThenInclude(x => x.Certification).Where(x => x.UserId == userId && x.Id == item.paymentForId).Select(x => x.CertificationPriceOption.Certification.ShortCode).FirstOrDefaultAsync();
                        //    //}
                        //    //if (item.paymentfor == paymentForEnums.Data)
                        //    //{
                        //    //    item.paymentForName = await _context.UserData.Include(x => x.Data).Where(x => x.UserId == userId && x.Id == item.paymentForId).Select(x => x.Data.NetworkProvider + " " + x.Data.Bundle).FirstOrDefaultAsync();
                        //    //}
                        //    //if (item.paymentfor == paymentForEnums.Modem)
                        //    //{
                        //    //    item.paymentForName = await _context.UserModem.Include(x => x.Modem).Where(x => x.UserId == userId && x.Id == item.paymentForId).Select(x => x.Modem.NetworkProvider + " " + x.Modem.Bundle).FirstOrDefaultAsync();
                        //    //}
                        //    //if (item.paymentfor == paymentForEnums.PhysicalCertificate)
                        //    //{
                        //    //    item.paymentForName = "Physical certificate";
                        //    //}
                        //    //if (item.paymentfor == paymentForEnums.Courier)
                        //    //{
                        //    //    var delivery = await _context.States.Include(x => x.Country).Where(x => x.Id == CourierStateId).Select(x => new { Country = x.Country.Name, state = x.Name }).FirstOrDefaultAsync();
                        //    //   if(delivery != null)
                        //    //    {
                        //    //        item.paymentForName = delivery.Country + "/" + delivery.state;
                        //    //    }


                        //    //}
                        //}
                    }

                    var userDevices = await _context.UserDevices.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Select(x => new UserDevicesRecord
                    {
                        Type= x.Type,
                        PurchaseDate =x.RegisteredDate.ToString("dd/MM/yyyy"),
                        TypeId = x.TypeId,
                        UserDeviceId =x.Id

                    }).Take(5).ToListAsync();
                    if(userDevices.Count()>0)
                    {
                        foreach (var item in userDevices)
                        {
                            if (item.Type.ToLower() == "modem")
                            {
                                var device = await _context.tblModem.Where(x => x.Id == item.TypeId).FirstOrDefaultAsync();
                                if (device != null)
                                {
                                    item.Manufacturer = device.Manufacturer;
                                    if (device.Amount.ToLower() != "free")
                                    {
                                        item.Amount = "₦ " + Convert.ToDouble(device.Amount).ToString("N");
                                    }
                                    else
                                    {
                                        item.Amount = device.Amount;
                                    }
                                }
                            }
                        }

                    }

                    //Load Programs from DB
                    //-------------------------------
                    IEnumerable<Programs> programList = await _context.Programs.ToListAsync();
                    var courseListVm = new UserCoursesVM()
                    {
                        UserCourseList = userCour
                    };
                    var result = new StudentDashboardVM();
                    result.fullName = existingUser.FirstName + " " + existingUser.LastName;
                    result.studentNumber = existingUser.StudentNumber;
                    result.UserCoursesVM = courseListVm;
                    result.UserCertificationsList = userCert;
                    result.UserDataList = data;
                    result.PaymentRecord = userPay;
                    result.UserDevicesList = userDevices;
                    result.programListz = programList;
                    GeneralClass.stNumber = existingUser.StudentNumber;
                    GeneralClass.email = existingUser.Email;
                    GeneralClass.FullName = existingUser.FirstName + " " + existingUser.LastName ;

                    
                    // Getting Server Address
                    //-----------------------
                    //string ipAddress = "";
                    //var IPs = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, address => address.AddressFamily == AddressFamily.InterNetwork);

                    //if (IPs.Length > 0)
                    //{
                    //    ipAddress = IPs[0].ToString();
                    //}

                    var login = new LoginTrail
                    {
                        LogTime = DateTime.Now,
                        UserId = existingUser.Id,
                        IP = IP4
                    };

                    await _context.LoginTrail.AddAsync(login);
                    await _context.SaveChangesAsync();

                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception Ex)
            {
                var mes = Ex.Message;
                throw Ex;
            }
            
        }
        private string GenerateReferalCode()
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

        public async Task<PaymentInitialiseResponse> InitializeCardPayment(PaymentDTO dto, PaymentMethodEnums paymentMethod, string redirectURL)
        {
            try
            {                
                using (var httpClientHandler = new HttpClientHandler())
                {
                    using (var _apiClient = new HttpClient(httpClientHandler))
                    {
                        // Users user = await _userManager.FindByIdAsync(dto.userId);
                        //Add discount code
                        //-----------------------------
                        
                        var subRef = $"{DateTime.Now.Ticks}{(int)paymentMethod}";

                        if (dto.ReferralDiscountCode != "" && dto.ReferralDiscountCode != null)
                        {
                            //var disCode = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();

                            var DiscountOwnerId = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();
                            if (DiscountOwnerId != null)
                            {
                                if (DiscountOwnerId.TotalApplied < DiscountOwnerId.TotalApproved)
                                {
                                    //Add discount details into DB
                                    //----------------------------
                                    var programPrice = await _context.UserProgramOption.Include(x => x.ProgramOption).Where(x => x.Id == dto.userProgramOptionId).Select(x => x.ProgramOption.PriceNGN).FirstOrDefaultAsync();
                                    var discount = (programPrice * DiscountOwnerId.Rate) / 100;
                                    DiscountOwnerId.TotalApplied = DiscountOwnerId.TotalApplied + 1;
                                    _context.UserDiscount.Update(DiscountOwnerId);

                                    if((float)dto.Amount >= programPrice)
                                    {
                                        dto.Amount = dto.Amount - (decimal)discount;
                                    }

                                    //Add to discount history
                                    //-----------------------
                                    var hist = new DiscountUsageHistory()
                                    {
                                        Earnings = discount,
                                        UsedByProgramOptionId = dto.userProgramOptionId,
                                        UserDiscountId = DiscountOwnerId.Id,
                                    };

                                    await _context.DiscountUsageHistory.AddAsync(hist);

                                    UserPaymentHistory paymentHistroyDis = new UserPaymentHistory
                                    {
                                        //UserProgramOptionId = dto.userProgramOptionId,
                                        Amount = (decimal)discount,
                                        PaymentRef = subRef,
                                        Description = "Discount",
                                        StatusId = PaymentStatusEnums.Paid,
                                        PaymentMethodId = PaymentMethodEnums.Discount,
                                        PaymentDate = DateTime.Now,
                                    };
                                    _context.UserPaymentHistory.Add(paymentHistroyDis);

                                    await _context.SaveChangesAsync();
                                }

                            }
                            else
                            {
                                // Add referral code to DB
                                //------------------------
                                var OwnerId = await _context.users.Where(x => x.ReferralCode == dto.ReferralDiscountCode && x.RegisteredDate.Date.AddMonths(3) >= DateTime.Now.Date).Select(x => x.Id).FirstOrDefaultAsync();
                                if (OwnerId != null && OwnerId != "")
                                {
                                    var userRef = new UserReferred()
                                    {
                                        ReferralId = OwnerId,
                                       // ReferredUserCourseId = dto.userProgramOptionId,
                                        //PaymentRef = subRef,
                                        //Earnings = 0,
                                    };
                                    await _context.UserReferred.AddAsync(userRef);
                                }
                            }

                        }
                        
                        UserPaymentHistory paymentHistroy = new UserPaymentHistory
                        {
                           // UserProgramOptionId = dto.userProgramOptionId,
                            Amount = dto.Amount,
                            PaymentRef = subRef,
                            Description = "",
                            StatusId =PaymentStatusEnums.Initialized,
                            PaymentMethodId = dto.PaymentMethod,
                            PaymentDate = DateTime.Now,
                        };
                        _context.UserPaymentHistory.Add(paymentHistroy);
                        await _context.SaveChangesAsync();

                        string[] paymentMethods = { paymentMethod.ToDescription().Replace(" ", "_").ToUpper() };
                        var user = await _context.UserProgramOption.Include(x => x.User).Where(x => x.Id == dto.userProgramOptionId).FirstOrDefaultAsync();
                        MonnifyPaymentInitializationRequest request = new MonnifyPaymentInitializationRequest
                        {
                            Amount = paymentHistroy.Amount,
                            CustomerName = $"{user.User.LastName } {user.User.FirstName}",
                            CustomerEmail = user.User.Email,
                            PaymentReference = paymentHistroy.PaymentRef,
                            PaymentDescription = "Payment",
                            CurrencyCode = "NGN",
                            ContractCode = _apiSettings.ContractCode,
                            RedirectUrl = $"{_apiSettings.PaymentRedirectUrl}/{redirectURL}?paymentRef={subRef}",
                            PaymentMethods = paymentMethods
                        };

                        var authenticationString = $"{_apiSettings.MonnifyKey}:{_apiSettings.MonnifySecret}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
                        var initContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/v1/merchant/transactions/init-transaction");
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                        requestMessage.Content = initContent;
                        var initResponse = await _apiClient.SendAsync(requestMessage);
                        var jsonInitResponse = await initResponse.Content.ReadAsStringAsync();
                        MonnifyPaymentInitializationResponse initDeserializeResponse = JsonConvert.DeserializeObject<MonnifyPaymentInitializationResponse>(jsonInitResponse);

                        if (initDeserializeResponse.RequestSuccessful)
                        {
                            //paymentHistroy.CheckoutUrl = initDeserializeResponse.ResponseDetails.CheckoutUrl;
                            paymentHistroy.Description = initDeserializeResponse.ResponseDetails.TransactionReference;
                            paymentHistroy.StatusId = PaymentStatusEnums.Pending;
                            await _context.SaveChangesAsync();

                            var response = new PaymentInitialiseResponse()
                            {
                                checkOutURL = initDeserializeResponse.ResponseDetails.CheckoutUrl,
                                paymentRef = subRef
                            };
                            return response;

                        }
                        //initDeserializeResponse.premium = premium;
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PaymentInitialiseResponse> InitializeAccountTransferPayment(PaymentDTO dto, PaymentMethodEnums paymentMethod, string redirectURL)
        {
            try
            {
                
                using (var httpClientHandler = new HttpClientHandler())
                {
                    using (var _apiClient = new HttpClient(httpClientHandler))
                    {
                        var subRef = $"{DateTime.Now.Ticks}{(int)paymentMethod}";
                        //Users user = await _userManager.FindByIdAsync(dto.userId);
                        
                        if (dto.ReferralDiscountCode != "" && dto.ReferralDiscountCode != null)
                        {
                            //var disCode = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();

                            var DiscountOwnerId = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();
                            if (DiscountOwnerId != null)
                            {
                                if (DiscountOwnerId.TotalApplied < DiscountOwnerId.TotalApproved)
                                {
                                    //Add discount details into DB
                                    //----------------------------
                                    var programPrice = await _context.UserProgramOption.Include(x => x.ProgramOption).Where(x => x.Id == dto.userProgramOptionId).Select(x => x.ProgramOption.PriceNGN).FirstOrDefaultAsync();
                                    var discount = (programPrice * DiscountOwnerId.Rate) / 100;
                                    DiscountOwnerId.TotalApplied = DiscountOwnerId.TotalApplied + 1;
                                    _context.UserDiscount.Update(DiscountOwnerId);

                                    if ((float)dto.Amount >= programPrice)
                                    {
                                        dto.Amount = dto.Amount - (decimal)discount;
                                    }

                                    //Add to discount history
                                    //-----------------------
                                    var hist = new DiscountUsageHistory()
                                    {
                                        Earnings = discount,
                                        UsedByProgramOptionId = dto.userProgramOptionId,
                                        UserDiscountId = DiscountOwnerId.Id,
                                    };

                                    await _context.DiscountUsageHistory.AddAsync(hist);

                                    UserPaymentHistory paymentHistroyDis = new UserPaymentHistory
                                    {
                                       // UserProgramOptionId = dto.userProgramOptionId,
                                        Amount = (decimal)discount,
                                        PaymentRef = subRef,
                                        Description = "Discount",
                                        StatusId = PaymentStatusEnums.Paid,
                                        PaymentMethodId = PaymentMethodEnums.Discount,
                                        PaymentDate = DateTime.Now,
                                    };
                                    _context.UserPaymentHistory.Add(paymentHistroyDis);

                                    await _context.SaveChangesAsync();
                                }

                            }
                            else
                            {
                                // Add referral code to DB
                                //------------------------
                                var OwnerId = await _context.users.Where(x => x.ReferralCode == dto.ReferralDiscountCode && x.RegisteredDate.Date.AddMonths(3) >= DateTime.Now.Date).Select(x => x.Id).FirstOrDefaultAsync();
                                if (OwnerId != null && OwnerId != "")
                                {
                                    var userRef = new UserReferred()
                                    {
                                        ReferralId = OwnerId,
                                       // ReferredUserCourseId = dto.userProgramOptionId,
                                        //PaymentRef = subRef,
                                        //Earnings = 0
                                    };
                                    await _context.UserReferred.AddAsync(userRef);
                                }
                            }
                        }
                        var user = await _context.UserProgramOption.Include(x => x.User).Where(x => x.Id == dto.userProgramOptionId).FirstOrDefaultAsync();
                      
                        UserPaymentHistory paymentHistroy = new UserPaymentHistory
                        {
                           // UserProgramOptionId = dto.userProgramOptionId,
                            Amount = dto.Amount,
                            PaymentRef = subRef,
                            Description = "",
                            StatusId = PaymentStatusEnums.Initialized,
                            PaymentMethodId = dto.PaymentMethod,
                            PaymentDate = DateTime.Now
                        };
                        _context.UserPaymentHistory.Add(paymentHistroy);
                        await _context.SaveChangesAsync();

                        string[] paymentMethods = { paymentMethod.ToDescription().Replace(" ", "_").ToUpper() };
                        MonnifyPaymentInitializationRequest request = new MonnifyPaymentInitializationRequest
                        {
                            Amount = paymentHistroy.Amount,
                            CustomerName = $"{user.User.LastName } {user.User.FirstName}",
                            CustomerEmail = user.User.Email,
                            PaymentReference = paymentHistroy.PaymentRef,
                            PaymentDescription = "Payment",
                            CurrencyCode = "NGN",
                            ContractCode = _apiSettings.ContractCode,
                            RedirectUrl = $"{_apiSettings.PaymentRedirectUrl}/{redirectURL}?paymentRef={subRef}",
                            PaymentMethods = paymentMethods
                        };


                        var authenticationString = $"{_apiSettings.MonnifyKey}:{_apiSettings.MonnifySecret}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
                        var initContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiSettings.MonnifyBaseUrl}/v1/merchant/transactions/init-transaction");
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                        requestMessage.Content = initContent;
                        var initResponse = await _apiClient.SendAsync(requestMessage);
                        var jsonInitResponse = await initResponse.Content.ReadAsStringAsync();
                        MonnifyPaymentInitializationResponse initDeserializeResponse = JsonConvert.DeserializeObject<MonnifyPaymentInitializationResponse>(jsonInitResponse);

                        if (initDeserializeResponse.RequestSuccessful)
                        {
                            paymentHistroy.Description = initDeserializeResponse.ResponseDetails.TransactionReference;
                            paymentHistroy.StatusId = PaymentStatusEnums.Pending;
                            await _context.SaveChangesAsync();

                            var response = new PaymentInitialiseResponse()
                            {
                                checkOutURL = initDeserializeResponse.ResponseDetails.CheckoutUrl,
                                paymentRef = subRef
                            };
                            return response;

                        }
                        
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GenerateMonoPaymentRef()
        {
            return DateTime.UtcNow.Ticks.ToString().Substring(8);
        }
        public async Task<PaymentInitialiseResponse> InitializeBankCOnnectPayment(PaymentDTO dto, string redirectURL)
        {
            try
            {

                var user = await _context.UserProgramOption.Include(x => x.User).Where(x => x.Id == dto.userProgramOptionId).FirstOrDefaultAsync();


                string MonoType = "onetime-debit"; 

                        string PaymentRef = GenerateMonoPaymentRef();

                if (dto.ReferralDiscountCode != "" && dto.ReferralDiscountCode != null)
                {
                    //var disCode = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();

                    var DiscountOwnerId = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();
                    if (DiscountOwnerId != null)
                    {
                        if (DiscountOwnerId.TotalApplied < DiscountOwnerId.TotalApproved)
                        {
                            //Add discount details into DB
                            //----------------------------
                            var programPrice = await _context.UserProgramOption.Include(x => x.ProgramOption).Where(x => x.Id == dto.userProgramOptionId).Select(x => x.ProgramOption.PriceNGN).FirstOrDefaultAsync();
                            var discount = (programPrice * DiscountOwnerId.Rate) / 100;
                            DiscountOwnerId.TotalApplied = DiscountOwnerId.TotalApplied + 1;
                            _context.UserDiscount.Update(DiscountOwnerId);

                            if ((float)dto.Amount >= programPrice)
                            {
                                dto.Amount = dto.Amount - (decimal)discount;
                            }

                            //Add to discount history
                            //-----------------------
                            var hist = new DiscountUsageHistory()
                            {
                                Earnings = discount,
                                UsedByProgramOptionId = dto.userProgramOptionId,
                                UserDiscountId = DiscountOwnerId.Id,
                            };

                            await _context.DiscountUsageHistory.AddAsync(hist);

                            UserPaymentHistory paymentHistroyDis = new UserPaymentHistory
                            {
                                //UserProgramOptionId = dto.userProgramOptionId,
                                Amount = (decimal)discount,
                                PaymentRef = PaymentRef,
                                Description = "Discount",
                                StatusId = PaymentStatusEnums.Paid,
                                PaymentMethodId = PaymentMethodEnums.Discount,
                                PaymentDate = DateTime.Now
                            };
                            _context.UserPaymentHistory.Add(paymentHistroyDis);

                            await _context.SaveChangesAsync();
                        }

                    }
                    else
                    {
                        // Add referral code to DB
                        //------------------------
                        var OwnerId = await _context.users.Where(x => x.ReferralCode == dto.ReferralDiscountCode && x.RegisteredDate.Date.AddMonths(3) >= DateTime.Now.Date).Select(x => x.Id).FirstOrDefaultAsync();
                        if (OwnerId != null && OwnerId != "")
                        {
                            var userRef = new UserReferred()
                            {
                                ReferralId = OwnerId,
                               // ReferredUserCourseId = dto.userProgramOptionId,
                                //PaymentRef = PaymentRef,
                               // Earnings = 0
                            };
                            await _context.UserReferred.AddAsync(userRef);
                        }
                    }
                }
                UserPaymentHistory SubpaymentHistroy = new UserPaymentHistory
                        {
                           // UserProgramOptionId = dto.userProgramOptionId,
                            Amount = dto.Amount,
                            PaymentRef = PaymentRef,
                            Description = "",
                            StatusId = PaymentStatusEnums.Initialized,
                            PaymentMethodId = dto.PaymentMethod,
                            PaymentDate = DateTime.Now
                };
                        _context.UserPaymentHistory.Add(SubpaymentHistroy);

                        await _context.SaveChangesAsync();

                        var AmountKobo = dto.Amount * 100;

                        MonocoParametersRequest Monorequest = new MonocoParametersRequest
                        {
                            amount = AmountKobo.ToString(),
                            description = "Payment with bank connect",
                            type = MonoType,
                            reference = PaymentRef,
                            redirect_url = $"{_apiSettings.PaymentRedirectUrl }/{redirectURL}"
                        };

                        var client = new RestClient($"{_apiSettings.MonocoBaseUrl }/v1/payments");
                        var request = new RestRequest("initiate", Method.Post);
                        request.AddHeader("Accept", "application/json");
                        request.AddHeader("mono-sec-key", $"{_apiSettings.MonocoSecretKey }");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", $"{JsonConvert.SerializeObject(Monorequest)}", ParameterType.RequestBody);
                        RestResponse response = client.Execute(request);

                        MonoPaymentInitializationResponse initDeserializeResponse = new MonoPaymentInitializationResponse();
                        if (response.Content != "")
                        {
                            initDeserializeResponse = JsonConvert.DeserializeObject<MonoPaymentInitializationResponse>(response.Content);
                            initDeserializeResponse.amount = (decimal.Parse(initDeserializeResponse.amount) / 100).ToString();

                            var respons = new PaymentInitialiseResponse()
                            {
                                checkOutURL = initDeserializeResponse.payment_link,
                                paymentRef = PaymentRef
                            };
                            return respons;                   

                         }

                return null;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
        public async Task<PaymentInitialiseResponse> InitializeOfflinePayment(PaymentDTO dto, string redirectURL)
        {
            try
            {

                var user = await _context.UserProgramOption.Include(x => x.User).Where(x => x.Id == dto.userProgramOptionId).FirstOrDefaultAsync();

                var subRef = $"{DateTime.Now.Ticks}{(int)dto.PaymentMethod}";

                if (dto.ReferralDiscountCode != "" && dto.ReferralDiscountCode != null)
                {
                    //var disCode = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();

                    var DiscountOwnerId = await _context.UserDiscount.Where(x => x.Code == dto.ReferralDiscountCode).FirstOrDefaultAsync();
                    if (DiscountOwnerId != null)
                    {
                        if (DiscountOwnerId.TotalApplied < DiscountOwnerId.TotalApproved)
                        {
                            //Add discount details into DB
                            //----------------------------
                            var programPrice = await _context.UserProgramOption.Include(x => x.ProgramOption).Where(x => x.Id == dto.userProgramOptionId).Select(x => x.ProgramOption.PriceNGN).FirstOrDefaultAsync();
                            var discount = (programPrice * DiscountOwnerId.Rate) / 100;
                            DiscountOwnerId.TotalApplied = DiscountOwnerId.TotalApplied + 1;
                            _context.UserDiscount.Update(DiscountOwnerId);

                            if ((float)dto.Amount >= programPrice)
                            {
                                dto.Amount = dto.Amount - (decimal)discount;
                            }

                            //Add to discount history
                            //-----------------------
                            var hist = new DiscountUsageHistory()
                            {
                                Earnings = discount,
                                UsedByProgramOptionId = dto.userProgramOptionId,
                                UserDiscountId = DiscountOwnerId.Id,
                            };

                            await _context.DiscountUsageHistory.AddAsync(hist);

                            UserPaymentHistory paymentHistroyDis = new UserPaymentHistory
                            {
                                //UserProgramOptionId = dto.userProgramOptionId,
                                Amount = (decimal)discount,
                                PaymentRef = subRef,
                                Description = "Discount",
                                StatusId = PaymentStatusEnums.Paid,
                                PaymentMethodId = PaymentMethodEnums.Discount,
                                PaymentDate = DateTime.Now
                            };
                            _context.UserPaymentHistory.Add(paymentHistroyDis);

                            await _context.SaveChangesAsync();
                        }

                    }
                    else
                    {
                        // Add referral code to DB
                        //------------------------
                        var OwnerId = await _context.users.Where(x => x.ReferralCode == dto.ReferralDiscountCode && x.RegisteredDate.Date.AddMonths(3) >= DateTime.Now.Date).Select(x => x.Id).FirstOrDefaultAsync();
                        if (OwnerId != null && OwnerId != "")
                        {
                            var userRef = new UserReferred()
                            {
                                ReferralId = OwnerId,
                                //ReferredUserCourseId = dto.userProgramOptionId,
                                //PaymentRef = subRef,
                                //Earnings = 0
                            };
                            await _context.UserReferred.AddAsync(userRef);
                        }
                    }
                }
                UserPaymentHistory SubpaymentHistroy = new UserPaymentHistory
                {
                    //UserProgramOptionId = dto.userProgramOptionId,
                    Amount = dto.Amount,
                    PaymentRef = subRef,
                    Description = "Offline",
                    StatusId = PaymentStatusEnums.Pending,
                    PaymentMethodId = dto.PaymentMethod,
                    PaymentDate = DateTime.Now
                };
                _context.UserPaymentHistory.Add(SubpaymentHistroy);

                await _context.SaveChangesAsync();

                var response = new PaymentInitialiseResponse()
                {
                    checkOutURL = "Successful",
                    paymentRef = subRef
                };
                return response;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        //public async Task<List<ProgramRecord>> UserPrograms(string email)
        //{
        //    //Load user programs
        //    //------------------
        //    var result = await _context.UserProgramOption.Include(x=>x.User).Include(x => x.ProgramOption).ThenInclude(x => x.Category).ThenInclude(x => x.Program).Where(x => x.User.Email == email).OrderByDescending(x=>x.Id).Select(x=> new ProgramRecord
        //    {
        //        ProgramOptionId =x.ProgramOption.ProgramOptionId,
        //        userProgramOptionId = x.Id,
        //        Name = x.ProgramOption.Category.Program.Name + "/" + x.ProgramOption.Name,
        //        StartDate = x.ProgramOption.StartDate.ToString("dd/MM/yyyy"),
        //        ProgramStatus = x.ProgramStatus,
        //        ProgramPaymentStatus = x.PaymentStatus,
        //        programPrice = x.ProgramOption.PriceNGN.ToString("N")

        //    }).ToListAsync();

        //    if (result.Count() > 0)
        //    {
        //        foreach (var item in result)
        //        {
        //            //var amountPaidperProg = await _context.UserPaymentHistory.Include(m => m.UserProgramOption).Where(m => m.UserProgramOptionId == item.userProgramOptionId && m.StatusId == PaymentStatusEnums.Paid).Select(m => m.Amount).SumAsync();
        //            //item.amountPaid = amountPaidperProg.ToString("N");
        //        }

        //    }
        //    return result;
        //}
        public async Task<ReferralUsage> UserReferralCodeUsage(string email)
        {
            //Load user programs
            //------------------
            var result = await _context.UserReferralPaymentHistory.Include(x=>x.UserRefer).ThenInclude(x => x.ReferredUser).Include(x => x.UserRefer).ThenInclude(x=>x.Referral).Where(x => x.UserRefer.Referral.Email == email).OrderByDescending(x => x.Id).Select(x => new ReferralUsage2
            {
                AmountPaid = ((x.Earning * 100)/10).ToString("N"),
                DateRegistered = x.UserRefer.ReferredUser.RegisteredDate.ToString("dd/MM/yyyy"),
                Earnings = x.Earning.ToString("N"),
                Fullname = x.UserRefer.ReferredUser.FirstName + " " + x.UserRefer.ReferredUser.LastName,
               // Program = x.ReferredUserCourses.CoursePriceOption.Course.Category.Institution.ShortName + "/" + x.ReferredUserCourses.CoursePriceOption.Course.Name
                //UserId = x.ReferredUserProgramOption.UserId,
                //Fullname = x.ReferredUserProgramOption.User.FirstName + " " + x.ReferredUserProgramOption.User.LastName,
                //userProOptionId = x.ReferredUserProgramOptionId,
                //earnings = x.Earnings,
                //rCode = x.Referral.ReferralCode

            }).ToListAsync();

            var resultList = new ReferralUsage();

            if (result.Count() > 0)
            {

               // var resultList2 = new List<ReferralUsage2>();
                //foreach (var item in result)
                //{
                //    //User Payment and Program
                //    //-------------------------
                //    var uProgPayment = await _context.UserPaymentHistory.Include(x => x.UserProgramOption).ThenInclude(x=>x.ProgramOption).ThenInclude(x=>x.Program).Where(x => x.UserProgramOptionId == item.userProOptionId && x.StatusId == PaymentStatusEnums.Paid).OrderByDescending(x=>x.Id).ToListAsync();

                //    if(uProgPayment.Count() >0)
                //    {
                //        foreach(var re in uProgPayment)
                //        {
                //            var refee = new ReferralUsage2
                //            {
                //                AmountPaid = re.Amount.ToString("N"),
                //                DateRegistered = re.UserProgramOption.RegDate.ToShortDateString(),
                //                Earnings = item.earnings.ToString("N"),
                //                Fullname =item.Fullname,
                //                Program = re.UserProgramOption.ProgramOption.Program.Name + "/" + re.UserProgramOption.ProgramOption.Name

                //            };
                //            resultList2.Add(refee);
                //        }

                //    }
                //    resultList.rCode = item.rCode;
                //}
                resultList.rCode = await _context.UserReferred.Include(x => x.Referral).Where(x => x.Referral.Email == email).Select(x => x.Referral.ReferralCode).FirstOrDefaultAsync();
                
                resultList.ReferralUsage2 = result;
                
            }

            //Load user programs
            //------------------
           var resultDis = await _context.UserDiscount.Include(x => x.Referral).Where(x => x.Referral.Email == email).OrderByDescending(x => x.Id).ToListAsync();
           if(resultDis.Count() > 0)
            {
                resultList.Discount = resultDis;
            }
            return resultList;
        }
        public async Task<ListDiscountHisto> DiscountHistory(string code)
        {
            //Load user programs
            //------------------
            var result = await _context.DiscountUsageHistory.Include(x => x.UsedByProgramOption).ThenInclude(x => x.User).Include(x=>x.UsedByProgramOption).ThenInclude(x=>x.ProgramOption).ThenInclude(x => x.Category).ThenInclude(x=>x.Institution).Include(x=>x.UserDiscount).Where(x => x.UserDiscount.Code == code).OrderByDescending(x => x.Id).Select(x => new DiscountHisto
            {
                Fullname = x.UsedByProgramOption.User.FirstName + " " + x.UsedByProgramOption.User.LastName,
                Earnings = x.Earnings.ToString("N"),
                DateRegistered = x.UsedByProgramOption.RegDate.ToString("dd/MM/yyyy"),
                Program = x.UsedByProgramOption.ProgramOption.Category.Institution.ShortName + "/" + x.UsedByProgramOption.ProgramOption.Name,
                ProgramFee = x.UsedByProgramOption.ProgramOption.PriceNGN.ToString("N")

            }).ToListAsync();

            var resultDis = new ListDiscountHisto();
            resultDis.DiscountHisto = result;
            resultDis.code = code;
            return resultDis;
        }
        public async Task<List<PaymentRecord>> UserPaymentHistories(string userId)
        {
            var payment = await _context.UserPaymentHistory.Where(x => x.UserId == userId && x.Description != "Changed Date").OrderByDescending(x => x.Id).Select(x => new
            {
                Amount = x.Amount,//"₦ " + x.Amount.ToString("N"),
                PaymentDate = x.PaymentDate,
                PaymentMethod = x.PaymentMethodId,
                PaymentRef = x.PaymentRef,
                Status = x.StatusId,
                paymentfor = x.PaymentFor,
                paymentForId = x.UserPaymentForId

            }).ToListAsync();
            var userPay = new List<PaymentRecord>();
            if (payment.Count() > 0)
            {
                var eachTransId = payment.GroupBy(i => i.PaymentRef).Select(group => new
                {
                    pRef = group.Key,
                    TotalAmount = group.Sum(g => g.Amount),
                    PaymentDate = group.Select(x => x.PaymentDate),
                    PaymentMethod = group.Select(x => x.PaymentMethod),
                });

                foreach (var item in eachTransId)
                {

                    var eachUserPay = new PaymentRecord
                    {
                        Amount = "₦ " + item.TotalAmount.ToString("N"),
                        PaymentDate = item.PaymentDate.FirstOrDefault(),
                        PaymentMethod = item.PaymentMethod.FirstOrDefault(),
                        PaymentRef = item.pRef
                    };

                    //Getting the payment status of course
                    //------------------------------------
                    foreach (var item1 in payment)
                    {
                        if (item1.PaymentRef == item.pRef && item1.paymentfor == paymentForEnums.Course)
                        {
                            eachUserPay.Status = item1.Status;
                        }
                    }
                    userPay.Add(eachUserPay);
                }

            }

            return userPay;
        }
        public async Task<List<PaymentRecord>> EachPaymentHistories(string userId, string PaymentRef)
        {
            var payment = await _context.UserPaymentHistory.Where(x => x.UserId == userId && x.PaymentRef == PaymentRef).OrderByDescending(x => x.Id).Select(x => new PaymentRecord
            {
                Amount ="₦ " + x.Amount.ToString("N"),
                PaymentDate = x.PaymentDate,
                PaymentMethod = x.PaymentMethodId,
                PaymentRef = x.PaymentRef,
                Status = x.StatusId,
                paymentfor = x.PaymentFor,
                paymentForId = x.UserPaymentForId                

            }).ToListAsync();

            if (payment.Count() > 0)
            {
                foreach (var item in payment)
                {
                    int courierState = 0;
                    if (item.paymentfor == paymentForEnums.Course)
                    {
                        if(item.PaymentMethod == PaymentMethodEnums.Referral)
                        {
                            item.paymentForName = "Referral";
                        }
                        else if (item.PaymentMethod == PaymentMethodEnums.Discount)
                        {
                            item.paymentForName = "Discount";
                        }
                        else
                        {
                            var re = await _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).Where(c => c.Id == item.paymentForId).Select(c => new { name = c.CoursePriceOption.Course.CourseCode, courierst = c.deliveryStateId }).FirstOrDefaultAsync();

                            if (re != null)
                            {
                                item.paymentForName = re.name.Length <= 20 ? re.name : re.name.Substring(0, 20) + "..."; ;
                                courierState = re.courierst;
                            }
                        }
                        
                    }
                    if (item.paymentfor == paymentForEnums.Certifications)
                    {
                        item.paymentForName = await _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).Where(c => c.Id == item.paymentForId).Select(c => c.CertificationPriceOption.Certification.Name).FirstOrDefaultAsync();
                    }
                    if (item.paymentfor == paymentForEnums.Data )
                    {
                        item.paymentForName = await _context.UserData.Include(c => c.Data).Where(c => c.Id == item.paymentForId).Select(c => c.Data.NetworkProvider).FirstOrDefaultAsync();
                    }
                    if (item.paymentfor == paymentForEnums.Modem)
                    {
                        item.paymentForName = await (from ud in _context.UserDevices
                                                     join m in _context.tblModem on ud.TypeId equals m.Id
                                                     where ud.Id == item.paymentForId
                                                     select m.Manufacturer).FirstOrDefaultAsync();
                    }
                    if (item.paymentfor == paymentForEnums.PhysicalCertificate)
                    {
                        item.paymentForName = "Physical Certificate";
                    }
                    if (item.paymentfor == paymentForEnums.Courier)
                    {
                        item.paymentForName ="Delivery to "+ await _context.States.Where(c => c.Id == courierState).Select(c => c.Name).FirstOrDefaultAsync();

                    }
                }
            }

            return payment;
        }
        private string GenerateReceiptId()
        {
            Random rd = new Random();

            string result = "";

            for (int i = 0; i < 6; i++)
            {
                result += rd.Next(10);
            }


           // result = "A-23" + result;

            return result;
        }
        public GenInvoice GenerateInvoice(string userId, string PaymentRef)
        {
            var getIn = new GenInvoice();
            var payment = _context.UserPaymentHistory.Where(x => x.UserId == userId && x.PaymentRef == PaymentRef).OrderByDescending(x => x.Id).Select(x => new PaymentRecord
            {
                Amount = x.Amount.ToString(),
                PaymentDate = x.PaymentDate,
                PaymentMethod = x.PaymentMethodId,
                PaymentRef = x.PaymentRef,
                Status = x.StatusId,
                paymentfor = x.PaymentFor,
                paymentForId = x.UserPaymentForId

            }).ToList();
            var insti = new Institutions();
            if (payment.Count() > 0)
            {
                int courseId = 0;
                var paymentref = ""; var paymetDate = new DateTime(); double totalP = 0;
                UserProgramPaymentStatusEnums coursePStatus = new UserProgramPaymentStatusEnums();
                foreach (var item in payment)
                {
                    int courierState = 0;
                    paymentref = item.PaymentRef;
                    paymetDate = item.PaymentDate;
                    totalP += Convert.ToDouble(item.Amount);
                    item.Amount = "₦ " + Convert.ToDouble(item.Amount).ToString("N");
                    
                    if (item.paymentfor == paymentForEnums.Course)
                    {
                        var re = _context.UserCourses.Include(c => c.CoursePriceOption).ThenInclude(c => c.Course).Where(c => c.Id == item.paymentForId).Select(c => new { name = c.CoursePriceOption.Course.Name, courierst = c.deliveryStateId, courseId = c.CoursePriceOption.CourseId, status = c.PaymentStatus }).FirstOrDefault();

                        if (re != null)
                        {
                            item.paymentForName = re.name;
                            courierState = re.courierst;
                            courseId = re.courseId;
                            coursePStatus = re.status;
                        }
                    }
                    if (item.paymentfor == paymentForEnums.Certifications)
                    {
                        item.paymentForName = _context.UserCertifications.Include(c => c.CertificationPriceOption).ThenInclude(c => c.Certification).Where(c => c.Id == item.paymentForId).Select(c => c.CertificationPriceOption.Certification.Name).FirstOrDefault();
                    }
                    if (item.paymentfor == paymentForEnums.Data)
                    {
                        item.paymentForName = _context.UserData.Include(c => c.Data).Where(c => c.Id == item.paymentForId).Select(c => c.Data.NetworkProvider).FirstOrDefault();
                    }
                    if (item.paymentfor == paymentForEnums.Modem)
                    {
                        item.paymentForName = (from ud in _context.UserDevices
                                                     join m in _context.tblModem on ud.TypeId equals m.Id
                                                     where ud.Id == item.paymentForId
                                                     select m.Manufacturer).FirstOrDefault();
                    }
                    if (item.paymentfor == paymentForEnums.PhysicalCertificate)
                    {
                        item.paymentForName = "Physical Certificate";
                    }
                    if (item.paymentfor == paymentForEnums.Courier)
                    {
                        item.paymentForName = "Delivery to " + _context.States.Where(c => c.Id == courierState).Select(c => c.Name).FirstOrDefault();

                    }
                }

                if(courseId > 0)
                {
                    insti = _context.Courses.Include(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.Id == courseId).Select(x=>x.Category.Institution).FirstOrDefault();
                }

                getIn.PaymentRecord = payment;
                getIn.paymentRef = paymentref;
                getIn.paymentDate = paymetDate.ToString("dd/MM/yyyy");
                getIn.totalPayment = "₦ " + totalP.ToString("N");
                getIn.Institutions = insti;
                getIn.CoursePaymentStatus = coursePStatus;
                //if (courseId >0)
                //{
                //    var studentInst = new Institutions();
                //    var institution = await _context.Courses.Include(x => x.Institution).Where(x => x.Id == courseId).FirstOrDefaultAsync();
                //    if(institution != null)
                //    {
                //        studentInst = institution.Institution;
                //        getIn.Institutions = studentInst;

                //    }
                //}

                var userdetails = _userManager.Users.Where(x=>x.Id == userId).FirstOrDefault();
                getIn.users = userdetails;
                var ReceiptNum = "EA-" + GenerateReceiptId();
                getIn.ReceiptNum = ReceiptNum;
                var rec = new Receipts
                {
                    ReceiptId = ReceiptNum,
                    PaymentRef = paymentref
                };

                _context.Receipts.Add(rec);
                _context.SaveChanges();
            }

            return getIn;
        }
        public async Task<List<UserCertificates>> UserCertificates(string email)
        {
            //Load user programs
            //------------------
            var result = await _context.UserProgramOption.Include(x=>x.User).Include(x => x.ProgramOption).ThenInclude(x => x.Category).ThenInclude(x => x.Institution).Where(x => x.User.Email == email).Select(x => new UserCertificates
            {

            }).ToListAsync();

            return result;
        }
        public async Task<string> NewProgram(DashboardVM dto, string email)
        {
            Users user = await _userManager.FindByEmailAsync(email);
            //var progExist = await _context.UserProgramOption.Where(x => x.ProgramOptionId == dto.programOptionId && x.UserId == user.Id).Select(x=>x.Id).FirstOrDefaultAsync();
            //if(progExist <= 0)
            //{
            //    var userPro = new UserProgramOption()
            //    {
            //        ProgramOptionId = dto.programOptionId,
            //        UserId = user.Id,
            //        ProgramStatus = UserProgramStatusEnums.Pending,
            //        RegDate = DateTime.Now
            //    };
            //    await _context.UserProgramOption.AddAsync(userPro);

            //}
            //if ( await _context.SaveChangesAsync() >0)
            //{
            //    return "successful";
            //}
            return "Error";
        }
        public async Task<List<ProgramCategory>> GetProgramCatByProgramId(int ProgramId)
        {

            //Load Program categories from DB
            //-------------------------------
            var programCategoryList = await _context.ProgramCategory.Where(x=>x.InstitutionId == ProgramId).ToListAsync();
            return programCategoryList;
        }
        public async Task<string> DeleteUserProgramOption(int Id)
        {
            var result = await _context.UserProgramOption.Where(x => x.Id == Id).FirstOrDefaultAsync();
            if(result != null)
            {
                _context.UserProgramOption.Remove(result);
            }
            await _context.SaveChangesAsync();
            return "Done";
        }
        public async Task<string> GetDiscountRate(string code)
        {
            var discountRate = await _context.UserDiscount.Where(x => x.Code == code).Select(x=>x.Rate).FirstOrDefaultAsync();
            
            return discountRate+"";
        }
    }
}
