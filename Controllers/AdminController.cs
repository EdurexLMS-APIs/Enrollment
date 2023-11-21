using AspNetCoreHero.ToastNotification.Abstractions;
using CPEA.Models;
using CPEA.Utilities;
using CPEA.Utilities.DTO;
using CPEA.Utilities.Interface;
using CPEA.Utilities.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace CPEA.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminServices _adminServices;
        private readonly IProjectServices _projectServices;
        private readonly INotyfService _notyfService;
        private readonly UserManager<Users> _userManager;
        private readonly IImageUpload _imageService;
        private const string SessionUsername = "";

        public AdminController(IImageUpload imageService, IAdminServices adminServices, INotyfService notyfService, IProjectServices projectServices, UserManager<Users> userManager)
        {
            _adminServices = adminServices;
            _projectServices = projectServices; 
            _notyfService = notyfService;
            _userManager = userManager;
            _imageService = imageService;
        }
        [HttpPost]
        public async Task<IActionResult> Export()
        {
            var AllP = await _adminServices.PartialEnrollment();
            List<object> customers = new List<object>();
            string csvName = DateTime.Now.ToString("dd/MM/yyyy");
            if (AllP.Count>0)
            {
              customers = AllP.ToList().Select(x=> new[] { x.StudentNumber, $"{x.FirstName} {x.LastName}",x.Email, x.Phone, x.RegisteredDate}).ToList<object>();
                
            }

            customers.Insert(0, new string[5] { "Student Number", "Full Name", "Email", "Phone", "Reg. Date" });


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < customers.Count; i++)
            {
                string[] customer = (string[])customers[i];
                for (int j = 0; j < customer.Length; j++)
                {
                    sb.Append(customer[j] + ',');
                }

                sb.Append("\r\n");

            }
            //_notyfService.Success("Export was successful", 5);

            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", csvName+".csv");
        }
        public async Task<IActionResult> Roles()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AdminRoles();
            var vm = new AdminRolesVM();
            vm.adminRolesList = response;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> EditRole(AdminRolesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.EditRole(dto.createAdminRoles);
            var vm = new AdminRolesVM();
            vm.adminRolesList = response;
            return View("Roles", vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddRole(AdminRolesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddRole(dto.createAdminRoles);
            var vm = new AdminRolesVM();
            vm.adminRolesList = response;
            return View("Roles", vm);
        }
        public async  Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.Users.Include(x => x.Role).Where(x => x.UserName == sessionUsername).FirstOrDefaultAsync();//.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            GeneralClass.source = "Dashboard";
            GeneralClass.email = user.Email;
            GeneralClass.role = user.Role.Name;
            var dashboardRecord = new DashboardVM();

            //var email = GeneralClass.email;

            var record = await _adminServices.DashboardRe(user.Email);
            if (record != null)
            {
                return View(record);
            }
            else
            {
                _notyfService.Error("Invalid user", 10);

                return RedirectToAction(nameof(Login));
            }
            
        }
        public async Task<IActionResult> chartUserbyRole()
        {
            var programList = await _adminServices.chartUserbyRole();

            return Json(programList);
        }
        public async Task<IActionResult> chartEnrollmentbyQueryString(string param)
        {
            var programList = await _adminServices.chartEnrollmentbyQueryString(param);

            return Json(programList);
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            GeneralClass.email = "";
            GeneralClass.role = "";
            GeneralClass.pRef = "";
            GeneralClass.FullName = "";
            var res = await _projectServices.Logout();
            return RedirectToAction("Login", "Admin", new { area = "" });

        }
        public IActionResult Login()
        {
            GeneralClass.email = "";
            GeneralClass.role = "";
            GeneralClass.pRef = "";
            GeneralClass.FullName = "";
            HttpContext.Session.Clear();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var userLogin = await _adminServices.Login(dto);
            if (userLogin.Item2 == "Successful")
            {
                var valll = userLogin.Item1.UserName.ToString();
                var rol = userLogin.Item1.Role.Name.ToString();
                HttpContext.Session.SetString(SessionUsername, userLogin.Item1.UserName.ToString());

                _notyfService.Success("Welcome back, " + userLogin.Item1.Email, 10);

                return RedirectToAction("Dashboard", "Admin", new { area = "" });
            }
            else if(userLogin.Item2 == "User Blocked")
            {
                _notyfService.Error("Your account is blocked, contact the administrator.", 10);
            }
            else
            {
                _notyfService.Error(userLogin.Item2, 10);
            }
            return View();
        }
        public async Task<IActionResult> Register2()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var vm = new AdminRegisVM();
            var role = await _adminServices.AdminRoles();
            vm.adminRoles = role;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Register2(AdminRegisVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var userLogin = await _adminServices.RegisterAdmin(dto);

            if (userLogin.status == "Successful")
            {
                _notyfService.Success(userLogin.email + " created successfully.", 10);
            }
            else
            {
                _notyfService.Error(userLogin.status, 10);
            }
            return View("RegisteredUsers");
        }
        public async Task<IActionResult> RegisteredAffiliates()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.RegisteredAffiliates();

            return View(response);
        }
        public async Task<IActionResult> AllReferred(string email)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.SingleUserReferred(email);

            return View(response);
        }
        public async Task<IActionResult> RegisteredUsers()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.RegisteredUsers();
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response;
            return View(vm);
        }
        public async Task<IActionResult> BlockedUsers()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.BlockedUsers();
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> BlockUser(RegisteredUsersVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.blockUserDTO.adminEmail = user.Email;
            var response = await _adminServices.BlockUser(blockdto.blockUserDTO);
            if(response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " has been blocked.", 10);
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response.Item1;
            return View("RegisteredUsers", vm);
        }
        [HttpPost]
        public async Task<IActionResult> UnBlockUser(RegisteredUsersVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.blockUserDTO.adminEmail = user.Email;
            var response = await _adminServices.UnBlockUser(blockdto.blockUserDTO);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " has been unblocked.", 10);
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response.Item1;
            return View("BlockedUsers", vm);
        }
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(RegisteredUsersVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.ResetUserPasswordDTO.adminEmail = user.Email;
            var response = await _adminServices.ResetUserPassword(blockdto.ResetUserPasswordDTO);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " password has been reset.", 10);
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response.Item1;
            return View("RegisteredUsers", vm);
        }
        public async Task<IActionResult> ResetPassword(string Email)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }

            var response = await _adminServices.ResetPassword(user.Email);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success("Your password has been reset.", 10);
                return RedirectToAction(nameof(Login));
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredUsersVM();
            vm.RegisteredUsers = response.Item1;
            return View("RegisteredUsers", vm);
        }
        public async Task<IActionResult> RegisteredStudents()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.RegisteredStudents();
            var vm = new RegisteredStudentsVM();
            vm.RegisteredStudents = response;
            return View(vm);
        }
        public async Task<IActionResult> BlockedStudents()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.BlockedStudents();
            var vm = new RegisteredStudentsVM();
            vm.RegisteredStudents = response;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> BlockStudent(RegisteredStudentsVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.blockStudentDTO.adminEmail = user.Email;
            var response = await _adminServices.BlockStudent(blockdto.blockStudentDTO);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " has been blocked.", 10);
               // return RedirectToAction(nameof(Login));
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredStudentsVM();
            vm.RegisteredStudents = response.Item1;
            return View("RegisteredStudents", vm);
        }
        [HttpPost]
        public async Task<IActionResult> UnBlockStudent(RegisteredStudentsVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.blockStudentDTO.adminEmail = user.Email;
            var response = await _adminServices.UnBlockStudent(blockdto.blockStudentDTO);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " has been unblocked.", 10);
                //return RedirectToAction(nameof(Login));
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredStudentsVM();
            vm.RegisteredStudents = response.Item1;
            return View("BlockedStudents", vm);
        }
        [HttpPost]
        public async Task<IActionResult> ResetStudentPassword(RegisteredStudentsVM blockdto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            blockdto.ResetUserPasswordDTO.adminEmail = user.Email;
            var response = await _adminServices.ResetStudentPassword(blockdto.ResetUserPasswordDTO);
            if (response.Item3 == "Successful")
            {
                _notyfService.Success(response.Item2.Email + " password has been reset.", 10);
                //return RedirectToAction(nameof(Login));
            }
            else
            {
                _notyfService.Error(response.Item3, 10);
            }
            var vm = new RegisteredStudentsVM();
            vm.RegisteredStudents = response.Item1;
            return View("RegisteredStudents", vm);
        }
        public async Task<IActionResult> SingleStudent(string email)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }

            var response = await _adminServices.SingleStudent(email);
            return View(response);
        }
        public async Task<IActionResult> CompletedEnrollment()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.Enrollment();
            return View(response);
        }
        public async Task<IActionResult> PartialEnrollment()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.PartialEnrollment();
            return View(response);
        }
        public async Task<IActionResult> Payments()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.Paymemts();
            return View(response);
        }
        public async Task<IActionResult> TransactionLog()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.TransactionLog();
            return View(response);
        }
       
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            dto.ConfirmPayment.AdminId = user.Id;
            var res = await _adminServices.ConfirmSinglePayment(dto.ConfirmPayment);
            if(res == "Successful")
            {
                _notyfService.Success("Payment confirmation done.", 10);
                return RedirectToAction("TransactionLog", "Admin", new { area = "" });
            }
            _notyfService.Error(res, 10);
            return RedirectToAction("TransactionLog", "Admin", new { area = "" });
        }
        public async Task<IActionResult> ProgramCategories()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.ProgramCategories();
            var programs = await _adminServices.Institutions();
            programs.Insert(0, new InstitutionRecord { Id = 0, Name = "Select an option" });

            var vm = new ProgramCategoryVM();
            vm.Category = response;
            vm.institutionListz = programs;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View(vm);
        }
        public async Task<IActionResult> ProgramCategoriesByInstitution(int institutionId)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var programList = await _adminServices.ProgramCategoriesByInstitutionId(institutionId);

            programList.Insert(0, new ProgramCategory { Id = 0, Name = "Select an option" });

            return Json(new SelectList(programList, "Id", "Name"));
        }
        public async Task<IActionResult> Programs()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            //var pCate =await _adminServices.ProgramCategories();
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            var response = await _adminServices.Programs();
            var vm = new ProgramVM();
            vm.ProgramList = response;
            return View(vm);
        }
        public async Task<IActionResult> ProgramFees()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.ProgramFees();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddProgram(ProgramVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddPrograms(dto.newProgram);
            var vm = new ProgramVM();
            vm.ProgramList = response;
            //var pCate = await _adminServices.ProgramCategories();
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View("Programs", vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddProgramCategory(ProgramCategoryVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddProgramCategory(dto.NewCategory);
            var programs = await _adminServices.Institutions();
            programs.Insert(0, new InstitutionRecord { Id = 0, Name = "Select an option" });

            var vm = new ProgramCategoryVM();
            vm.Category = response;
            vm.institutionListz = programs;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View("ProgramCategories", vm);
        }
        public async Task<IActionResult> ProgramOptions()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var pCate = await _adminServices.ProgramCategories();
            ViewBag.listCat = new SelectList(pCate, "Id", "Name");
            var pro = await _adminServices.Programs();
            ViewBag.listPro = new SelectList(pro, "Id", "Name");
            var response = await _adminServices.ProgramOptions();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddProgramOption(ProgramOptions dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddProgramOption(dto);

            var pCate = await _adminServices.ProgramCategories();
            ViewBag.listCat = new SelectList(pCate, "Id", "Name");
            var pro = await _adminServices.Programs();
            ViewBag.listPro = new SelectList(pro, "Id", "Name");
            return View(response);
        }
        public async Task<IActionResult> Courses()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.Courses();
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if (institutions.Count() > 0)
            {
                foreach (var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id = item.Id,
                        Name = item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
            vm.Courses = response;
            //vm.programListz = programs;
            vm.institutionListz = institutionDropList;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddCourse(CoursesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddCourse(dto.Course);
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if (institutions.Count() > 0)
            {
                foreach (var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id = item.Id,
                        Name = item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
            vm.Courses = response;
            //vm.programListz = programs;
            vm.institutionListz = institutionDropList;

            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View("Courses", vm);
        }
        [HttpPost]
        public async Task<IActionResult> EditCourse(CoursesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.EditCourse(dto.editCourseDTO);
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if(institutions.Count()>0)
            {
                foreach(var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id =item.Id,
                        Name =item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
            vm.Courses = response;
            //vm.programListz = programs;
            vm.institutionListz = institutionDropList;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View("Courses", vm);
        }
        public async Task<IActionResult> CourseOptions(int CourseId)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.CourseOptions(CourseId);
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if (institutions.Count() > 0)
            {
                foreach (var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id = item.Id,
                        Name = item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
           // vm.programListz = programs;
            vm.institutionListz = institutionDropList;
            vm.CoursePriceOptionVM = response;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddCoursePrice(CoursesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddCoursePrice(dto.NewPriceOption);
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if (institutions.Count() > 0)
            {
                foreach (var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id = item.Id,
                        Name = item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
           // vm.programListz = programs;
            vm.institutionListz = institutionDropList;
            vm.CoursePriceOptionVM = response;
            return View("CourseOptions", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourseOption(CoursesVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.EditCourseOption(dto.editCourseOptionDTO);
            var programs = await _adminServices.Programs();
            var institutions = await _adminServices.Institutions();

            var institutionDropList = new List<Institutions>();
            if (institutions.Count() > 0)
            {
                foreach (var item in institutions)
                {
                    var eachIns = new Institutions
                    {
                        Id = item.Id,
                        Name = item.Name
                    };
                    institutionDropList.Add(eachIns);
                }
            }
            var vm = new CoursesVM();
            //vm.programListz = programs;
            vm.institutionListz = institutionDropList;
            vm.CoursePriceOptionVM = response;
            return View("CourseOptions", vm);
        }

        public async Task<IActionResult> Certifications()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.Certifications();
            var programs = await _adminServices.ProgramCategories();
            var currency = await _adminServices.Currencys();
            var vm = new CertificationsVM();
            vm.Certifications = response;
            vm.programCatListz = programs;
            vm.currencyListz = currency;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddCertification(CertificationsVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddCertification(dto.Certification);
            var programs = await _adminServices.ProgramCategories();
            var currency = await _adminServices.Currencys();

            var vm = new CertificationsVM();
            vm.Certifications = response;
            vm.programCatListz = programs;
            vm.currencyListz = currency;
            //ViewBag.list = new SelectList(pCate, "Id", "Name");
            return View("Certifications", vm);
        }
        public async Task<IActionResult> CertificationOptions(int CertId)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.CertificationOptions(CertId);
            var vm = new CertificationsVM();
            vm.CertPriceOptionVM = response;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddCertPrice(CertificationsVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddCertificationPrice(dto.NewPriceOption);
            var vm = new CertificationsVM();
            vm.CertPriceOptionVM = response;
            return View("CertificationOptions", vm);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUser dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var userLogin = await _adminServices.EditUser(dto);
            //if (userLogin.status == "Successful")
            //{
            //    GeneralClass.email = dto.AdminRegis.Email;
            //    return RedirectToAction(nameof(Dashboard));
            //}
            return View("RegisteredUsers", userLogin);
        }
        public async Task<IActionResult> Report(ReportDTO dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            dto.From = null;
            dto.To = null;
            var vm = await _adminServices.Reports(dto);
            
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> ReportDateR(ReportDTO dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var vm = await _adminServices.Reports(dto);

            return View("Report", vm);
        }
        public async Task<IActionResult> Institutions()
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var vm = new InstitutionVM();
            vm.InstitutionList = await _adminServices.Institutions();

            //Load Countries from LocationAPI
            //-------------------------------
            var country = await _projectServices.GetCountries();
            //var allCountries = new List<CountryDetails>();
            //foreach (var item in country.data)
            //{
            //    var countryList = new CountryDetails()
            //    {
            //        Id = item.countryId,
            //        Name = item.countryName
            //    };

            //    allCountries.Add(countryList);
            //}

            //Load Nigeria States from LocationAPI
            //-------------------------------------
            var states = await _projectServices.GetStatesByCountryId(163);
            //var allstates = new List<StateDetails>();
            //foreach (var item in states.data.states)
            //{
            //    var stateList = new StateDetails()
            //    {
            //        Id = item.stateId,
            //        Name = item.stateName
            //    };

            //    allstates.Add(stateList);
            //}

            var InstType = await _adminServices.InstitutionTypes();

            vm.countryListz = country;
            vm.nigeriaStatesListz = states;
            vm.institutionTypesListz = InstType;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddInstitution(AddInstitutionDTO dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }

            string logoPath = null;

            if (dto.Logo != null)
            {
                var imageResult = await _imageService.UploadPhoto(dto.Logo, dto.ShortName);
               
                if (imageResult.Item1 != "Successful")
                {
                    _notyfService.Error(imageResult.Item1, 10);

                    return RedirectToAction(nameof(Institutions));

                }

                logoPath = imageResult.Item2;
            }

            var response = await _adminServices.AddInstitution(dto, logoPath);
            var vm = new InstitutionVM();
            //Load Countries from LocationAPI
            //-------------------------------
            var country = await _projectServices.GetCountries();
            //var allCountries = new List<CountryDetails>();
            //foreach (var item in country.data)
            //{
            //    var countryList = new CountryDetails()
            //    {
            //        Id = item.countryId,
            //        Name = item.countryName
            //    };

            //    allCountries.Add(countryList);
            //}

            //Load Nigeria States from LocationAPI
            //-------------------------------------
            var states = await _projectServices.GetStatesByCountryId(163);
            //var allstates = new List<StateDetails>();
            //foreach (var item in states.data.states)
            //{
            //    var stateList = new StateDetails()
            //    {
            //        Id = item.stateId,
            //        Name = item.stateName
            //    };

            //    allstates.Add(stateList);
            //}


            var InstType = await _adminServices.InstitutionTypes();

            vm.countryListz = country;
            vm.nigeriaStatesListz = states;
            vm.institutionTypesListz = InstType;
            vm.InstitutionList = response;

            return View("Institutions", vm);
        }
        [HttpPost]
        public async Task<IActionResult> EditInstitution(InstitutionVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.EditInstitution(dto.institutionEdit);
            var vm = new InstitutionVM();
            //Load Countries from LocationAPI
            //-------------------------------
            var country = await _projectServices.GetCountries();
            //var allCountries = new List<CountryDetails>();
            //foreach (var item in country.data)
            //{
            //    var countryList = new CountryDetails()
            //    {
            //        Id = item.countryId,
            //        Name = item.countryName
            //    };

            //    allCountries.Add(countryList);
            //}

            //Load Nigeria States from LocationAPI
            //-------------------------------------
            var states = await _projectServices.GetStatesByCountryId(163);
            //var allstates = new List<StateDetails>();
            //foreach (var item in states.data.states)
            //{
            //    var stateList = new StateDetails()
            //    {
            //        Id = item.stateId,
            //        Name = item.stateName
            //    };

            //    allstates.Add(stateList);
            //}


            var InstType = await _adminServices.InstitutionTypes();

            vm.countryListz = country;
            vm.nigeriaStatesListz = states;
            vm.institutionTypesListz = InstType;
            vm.InstitutionList = response;

            return View("Institutions", vm);
        }
        public async Task<IActionResult> DeleteInstitution(int Id)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.DeleteInstitution(Id);
            var vm = new InstitutionVM();
            //Load Countries from LocationAPI
            //-------------------------------
            var country = await _projectServices.GetCountries();
            //var allCountries = new List<CountryDetails>();
            //foreach (var item in country.data)
            //{
            //    var countryList = new CountryDetails()
            //    {
            //        Id = item.countryId,
            //        Name = item.countryName
            //    };

            //    allCountries.Add(countryList);
            //}

            //Load Nigeria States from LocationAPI
            //-------------------------------------
            var states = await _projectServices.GetStatesByCountryId(163);
            //var allstates = new List<StateDetails>();
            //foreach (var item in states.data.states)
            //{
            //    var stateList = new StateDetails()
            //    {
            //        Id = item.stateId,
            //        Name = item.stateName
            //    };

            //    allstates.Add(stateList);
            //}


            var InstType = await _adminServices.InstitutionTypes();

            vm.countryListz = country;
            vm.nigeriaStatesListz = states;
            vm.institutionTypesListz = InstType;
            vm.InstitutionList = response;

            return View("Institutions", vm);
        }
        public async Task<IActionResult> InstitutionStatus(int Id)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.InstitutionStatus(Id);
            var vm = new InstitutionVM();
            //Load Countries from LocationAPI
            //-------------------------------
            var country = await _projectServices.GetCountries();
            //var allCountries = new List<CountryDetails>();
            //foreach (var item in country.data)
            //{
            //    var countryList = new CountryDetails()
            //    {
            //        Id = item.countryId,
            //        Name = item.countryName
            //    };

            //    allCountries.Add(countryList);
            //}

            //Load Nigeria States from LocationAPI
            //-------------------------------------
            var states = await _projectServices.GetStatesByCountryId(163);
            //var allstates = new List<StateDetails>();
            //foreach (var item in states.data.states)
            //{
            //    var stateList = new StateDetails()
            //    {
            //        Id = item.stateId,
            //        Name = item.stateName
            //    };

            //    allstates.Add(stateList);
            //}


            var InstType = await _adminServices.InstitutionTypes();

            vm.countryListz = country;
            vm.nigeriaStatesListz = states;
            vm.institutionTypesListz = InstType;
            vm.InstitutionList = response;

            return View("Institutions", vm);
        }

        public async Task<IActionResult> InstitutionDestOffList(int InstitutionId)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.InstitutionDestOffList(InstitutionId);            
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddInstitutionDestOff(InstitutionDestOffVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.AddInstitutionDestOff(dto.addDeskOfficer);
            return View("InstitutionDestOffList", response);
        }
        [HttpPost]
        public async Task<IActionResult> EditInstitutionDestOff(InstitutionDestOffVM dto)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.EditInstitutionDestOff(dto.editDeskOfficer);
            return View("InstitutionDestOffList", response);
        }
        public async Task<IActionResult> DeleteInstitutionDestOff(int Id)
        {
            if (HttpContext.Session.GetString(SessionUsername) == null)
            {
                _notyfService.Error("Session time out", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var sessionUsername = HttpContext.Session.GetString(SessionUsername);
            var user = await _userManager.FindByNameAsync(sessionUsername);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Admin", new { area = "" });
            }
            var response = await _adminServices.DeleteInstitutionDestOff(Id);
            return View("InstitutionDestOffList", response);
        }
    }
}
