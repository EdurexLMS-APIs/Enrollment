using AspNetCoreHero.ToastNotification.Abstractions;
using CPEA.Models;
using CPEA.Utilities;
using CPEA.Utilities.Interface;
using CPEA.Utilities.ViewModel;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectServices _projectServices;
        private readonly INotyfService _notyfService;
        private readonly UserManager<Users> _userManager;
        private const string SessionUsername = "";
        private readonly RoleManager<IdentityRole> _roleManager;

        public EnrollmentController(RoleManager<IdentityRole> roleManager, ILogger<HomeController> logger, IProjectServices projectServices, INotyfService notyfService, UserManager<Users> userManager)
        {
            _logger = logger;
            _projectServices = projectServices;
            _notyfService = notyfService;
            _userManager = userManager;
            _roleManager = roleManager;
        }
       
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUsernameInUse(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                if (!username.Contains("@"))
                {
                    return Json(true);
                }
                else
                {
                    return Json($"({username}) can not have @ character");
                }
            }
            else
            {
                return Json($"({username}) is already in use");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Register(string code)
        {
            HttpContext.Session.Clear();

            var response = await _projectServices.GetRegister();
            if (code != null && code != "")
            {
                response.refCode = code;
                var Referral = await _userManager.Users.Where(x => x.ReferralCode == code).Select(x => x.DefaultRole).FirstOrDefaultAsync();
                if(Referral != null && Referral !="")
                {
                    var role = await _roleManager.FindByIdAsync(Referral);
                    if (role.Name == "Staff")
                    {
                        response.percentageOffer = "7%";
                    }
                    else if (role.Name == "Freelance")
                    {
                        response.percentageOffer = "5%";
                    }
                }
               
                _notyfService.Success($"Congratulations, you have {response.percentageOffer} off course price.", 10);

            }
            return View(response);
        }       
        [HttpPost]
        public async Task<IActionResult> Register(Register1VM dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Kindly input all required fields", 10);
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
            if(dto.registerz.accountType == null)
            {
                dto.registerz.accountType = "personal";
            }

            string url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            var paymentResponse = await _projectServices.Enrollment(dto.registerz,url );
            if(paymentResponse.Successful == true )
            {
                HttpContext.Session.SetString(SessionUsername, paymentResponse.Data.UserName.ToString());

                _notyfService.Success("Registration Successful", 10);
                //var response2 = await _projectServices.GetRegisterNew(paymentResponse.Data.Id);
                //response2.phoneNumber = paymentResponse.Data.PhoneNumber;
                //response2.refCode = dto.refCode;
                return RedirectToAction("Register2", "Enrollment", new { area = "" });
                //return View("Register2", response2);
            }
            else
            {
                _notyfService.Error(paymentResponse.Message, 10);
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
        }
        [HttpGet]
        public async Task<IActionResult> NYSCRegister()
        {
            HttpContext.Session.Clear();

            var response = await _projectServices.GetRegister();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> NYSCRegister(Register1VM dto)
        {
            if (!ModelState.IsValid)
            {
                var response1 = await _projectServices.GetRegister();
                _notyfService.Error("Kindly input all required fields", 10);
                return View("NYSCRegister", response1);
            }
            if (!dto.registerz.NYSCCallUpNumber.Contains("NYSC"))
            {
                var response1 = await _projectServices.GetRegister();
                _notyfService.Error("Invalid call up number", 10);
                return View("NYSCRegister", response1);
            }
            if (dto.registerz.accountType == null)
            {
                dto.registerz.accountType = "NYSC";
            }
            string url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            var paymentResponse = await _projectServices.Enrollment(dto.registerz, url);
            if (paymentResponse.Successful == true)
            {
                HttpContext.Session.SetString(SessionUsername, paymentResponse.Data.UserName.ToString());

                _notyfService.Success("Registration Successful", 10);
                //var response2 = await _projectServices.GetRegisterNew(paymentResponse.Data.Id);
                //response2.phoneNumber = paymentResponse.Data.PhoneNumber;
                return RedirectToAction("Register2", "Enrollment", new { area = "" });
                //return View("Register2", response2);
            }
            else 
            {
                _notyfService.Error(paymentResponse.Message, 10);
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
            //else
            //{
            //    _notyfService.Error("Error occured while creating your account", 10);
            //    var response1 = await _projectServices.GetRegister();
            //    return View("Register", response1);
            //}
        }
        [HttpGet]
        public async Task<IActionResult> Register2()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }

            var response = await _projectServices.GetRegisterNew(user.Id);
            if(GeneralClass.certOnly == true)
            {
                response.CertificationOnly = true;
            }

            if(response.refCode !=null && response.refCode != "")
            {
                if (response.percentageOffer == 0)
                {
                    var Referral = await _userManager.Users.Where(x => x.ReferralCode == response.refCode).Select(x => x.DefaultRole).FirstOrDefaultAsync();
                    if (Referral != null && Referral != "")
                    {
                        var role = await _roleManager.FindByIdAsync(Referral);
                        if (role.Name == "Staff")
                        {
                            response.percentageOffer = 7;
                        }
                        else if (role.Name == "Freelance")
                        {
                            response.percentageOffer = 5;
                        }
                    }
                }
               
                _notyfService.Success($"Congratulations, you have {response.percentageOffer}% off course price.", 10);

            }

            //response.phoneNumber = user.PhoneNumber;
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register2(RegisterNewVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
           // var user = await _userManager.FindByNameAsync(User.Identity.Name);
            dto.register2DTO.userId = user.Id;
            //dto.register2DTO.refCode = dto.refCode;
            //Interswitch Payment Integration
            //-------------------------------            
            //var paymentResponse = await _projectServices.EnrollmentInterswitch(dto.register2DTO);
            //if (paymentResponse != null && paymentResponse != "")
            //{
            //    // Next action
            //}

            //return View();
            //End of Interswitch Payment Integration
            //--------------------------------------

            //Monnify Payment Integration
            //---------------------------
            var paymentResponse = await _projectServices.Enrollment2(dto.register2DTO);
            if (paymentResponse != null)
            {
                if (paymentResponse.checkOutURL.Contains("htt"))
                {
                    GeneralClass.pRef = paymentResponse.paymentRef;
                    return new RedirectResult(paymentResponse.checkOutURL, true);
                }
                else if(paymentResponse.errorMessage != null && paymentResponse.errorMessage !="")
                {
                    _notyfService.Error(paymentResponse.errorMessage, 10);
                    var response2 = await _projectServices.GetRegisterNew(user.Id);
                    response2.phoneNumber = user.PhoneNumber;
                    return View("Register2", response2);
                }
                else
                {
                    _notyfService.Success("Welcome " + user.Email, 10);
                    return RedirectToAction("Dashboard", "Home", new { area = "" });
                }

            }

           // var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var response = await _projectServices.GetRegisterNew(user.Id);
            response.phoneNumber = user.PhoneNumber;
            return View("Register2", response);

            //return RedirectToAction(nameof(Register2));

            ////var paymentResponse = await _projectServices.Enrollment2(dto.register2DTO);
            ////if (paymentResponse != null)
            ////{
            ////    if (paymentResponse.checkOutURL.Contains("htt"))
            ////    {
            ////        GeneralClass.pRef = paymentResponse.paymentRef;
            ////       return new RedirectResult(paymentResponse.checkOutURL, true);
            ////    }

            ////    //_notyfService.Success("Saved Successfully", 5);
            ////    return RedirectToAction("Dashboard", "Home", new { area = "" });
            ////}

            ////var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ////var response = await _projectServices.GetRegisterNew();
            ////response.phoneNumber = user.PhoneNumber;
            ////return View("Register2", response);
            //return RedirectToAction(nameof(Register2));
        }

    }
}
