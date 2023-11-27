using AspNetCoreHero.ToastNotification.Abstractions;
using CPEA.Models;
using CPEA.Utilities;
using CPEA.Utilities.DTO;
using CPEA.Utilities.Interface;
using CPEA.Utilities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using iTextSharp.text.pdf;
using iTextSharp.text;
using CPEA.Data;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text.pdf.qrcode;
using Org.BouncyCastle.Ocsp;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SendGrid;
using System.Data;
//using DinkToPdf.Contracts;
//using DinkToPdf;

namespace CPEA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectServices _projectServices;
        private readonly INotyfService _notyfService;
        private readonly UserManager<Users> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmail _email;
        private const string SessionUsername = "";
        private readonly RoleManager<IdentityRole> _roleManager;

        //private IConverter _converter;

        public HomeController(RoleManager<IdentityRole> roleManager,IEmail email, ApplicationDbContext context, ILogger<HomeController> logger, IProjectServices projectServices, INotyfService notyfService, UserManager<Users> userManager)
        {
            _logger = logger;
            _projectServices = projectServices;
            _notyfService = notyfService;
            _userManager = userManager;
            _context = context;
            _email = email;
            _roleManager = roleManager;
            //_converter = converter;
        }
        public IActionResult ResetPassword(string token)
        {
            if (token == null || token == "")
            {
                _notyfService.Error("You can't reset password.", 10);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var req = new PasswordResetDTO();
            token = Decrypt(token);
            var email = token.Split(' ')[0];
            req.email = email;
           // req.ConfirmPassword = token.Split(' ')[1];
            return View(req);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(PasswordResetDTO dto)
        {

            if (dto.NewPassword == null || dto.NewPassword == "")
            {
                _notyfService.Error("Kindly input your new password.", 10);
                return RedirectToAction("Login", "Home", new { area = "" });

            }

            var response = await _projectServices.ResetPassword(dto);

            if (response == "Successful")
            {
                _notyfService.Success("Password was reset successfully.", 10);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            else
            {
                _notyfService.Error(response, 10);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
        }
        private static string Decrypt(string S)
        {
            string output = "";
            char[] readChar = S.ToCharArray();
            for (int i = 0; i < readChar.Length; i++)
            {
                int no = Convert.ToInt32(readChar[i]) - 10;
                string r = Convert.ToChar(no).ToString();
                output += r;
            }
            return output;
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string Email)
        {

            if (Email == null || Email == "" )
            {
                _notyfService.Error("Kindly input your email.", 10);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            string url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            var response = await _projectServices.ForgotPassword(Email, url);

            if (response.Item2 == true)
            {
                _notyfService.Success("Reset email has been sent to you.", 10);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            else             {
                _notyfService.Error(response.Item1, 10);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
        }
        public async Task<IActionResult> SendEmail()
        {
            var result = await _email.SendEmail("","","");
            return View();
        }
        public async Task<IActionResult> CourseRegConfirmation()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            return View();
        }
        [HttpPost("SendRegConfirmation")]
        public async Task<IActionResult> SendRegConfirmation(int courseId, string source)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            string Mode = "Email";
            var result =await _projectServices.SendRegConfirmation(user.Id, courseId, Mode);
            if(result.Item2 == "Successful")
            {
                _notyfService.Success("Email sent successfully", 10);
                return RedirectToAction(source, "Home", new { area = "" });
            }
            else if (result.Item2 == "Download")
            {
                _notyfService.Success("Downloaded", 10);
                return RedirectToAction(source, "Home", new { area = "" });

            }
            else
            {
                _notyfService.Error(result.Item2, 10);
                return RedirectToAction(source, "Home", new { area = "" });

            }
        }

        //[HttpPost("DownloadRegConfirmation")]
        //public async Task<IActionResult> DownloadRegConfirmation(int courseId, string source)
        //{
        //    var user = _userManager.Users.Where(x=>x.UserName == User.Identity.Name).FirstOrDefault();
            
        //    string Mode = "Download";
        //    var result = _projectServices.DownloadRegConfirmation(user.Id, courseId, Mode);
        //    if (result.Item2 == "Download")
        //    {
        //        PdfConfirmation(result.Item1, courseId);
        //        _notyfService.Success("Download was successful", 10);
        //       // return RedirectToAction(source, "Home", new { area = "" });

        //    }

        //    _notyfService.Error("Error while downloding", 10);
        //    return RedirectToAction(source, "Home", new { area = "" });

        //}

        public FileResult DownloadRegConfirmation(int courseId, string source)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            
            var user = _userManager.Users.Where(x => x.UserName == sessionValue).FirstOrDefault();

            string Mode = "Download";
            var response = _projectServices.DownloadRegConfirmation(user.Id, courseId, Mode);

            Document doc = new Document(PageSize.A4);
            MemoryStream workStream = new MemoryStream();
            string strPDFFileName = string.Format(response.courseCode + "-" + response.studentId + ".pdf");

            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            Paragraph paraTitle = new Paragraph("Course Registration Confirmation", FontFactory.GetFont("Arial", 15, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            paraTitle.Alignment = Element.ALIGN_CENTER;
            paraTitle.SpacingAfter = 15.5f;
            doc.Add(paraTitle);

            Paragraph para2 = new Paragraph("Dear " + response.studentName + ",", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para2.Alignment = Element.ALIGN_LEFT;
            para2.SpacingAfter = 10.5f;
            doc.Add(para2);

            var phrase11 = new Phrase("We are delighted to confirm your successful registration for the ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            var phrase21 = new Phrase(response.courseName, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            var phrase31 = new Phrase(" at EDUREX Academy. ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));

            Paragraph para3 = new Paragraph();
            para3.Alignment = Element.ALIGN_LEFT;
            para3.Add(phrase11);
            para3.Add(phrase21);
            para3.Add(phrase31);
            para3.SpacingAfter = 6.5f;

            doc.Add(para3);

            Paragraph para4 = new Paragraph("Below are the details of your registration:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para4.Alignment = Element.ALIGN_LEFT;
            para4.SpacingAfter = 12.5f;
            doc.Add(para4);

            Paragraph para5 = new Paragraph("Registration Details:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para5.Alignment = Element.ALIGN_LEFT;
            para5.SpacingAfter = 8.5f;
            doc.Add(para5);

            PdfPTable table1 = new PdfPTable(1);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.SpacingAfter = 10.5f;
            table1.WidthPercentage = 100;

            PdfPCell cellStdId = new PdfPCell();
            cellStdId.Border = Rectangle.NO_BORDER;
            cellStdId.AddElement(new Paragraph("• Student ID: " + response.studentId, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellRegCou = new PdfPCell();
            cellRegCou.Border = Rectangle.NO_BORDER;
            cellRegCou.AddElement(new Paragraph("• Registered Course: " + response.courseName, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellstaDate = new PdfPCell();
            cellstaDate.Border = Rectangle.NO_BORDER;
            cellstaDate.AddElement(new Paragraph("• Start Date: " + response.startDate, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellendDate = new PdfPCell();
            cellendDate.Border = Rectangle.NO_BORDER;
            cellendDate.AddElement(new Paragraph("• End Date: " + response.endDate, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellAwa = new PdfPCell();
            cellAwa.Border = Rectangle.NO_BORDER;
            cellAwa.AddElement(new Paragraph("• Certificate Awarded by: (EDUREX / University of Ibadan)", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table1.AddCell(cellStdId);
            table1.AddCell(cellRegCou);
            table1.AddCell(cellstaDate);
            table1.AddCell(cellendDate);
            table1.AddCell(cellAwa);
            doc.Add(table1);

            Paragraph para6 = new Paragraph("Payment Status:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para6.Alignment = Element.ALIGN_LEFT;
            para6.SpacingAfter = 8.5f;
            doc.Add(para6);

            PdfPTable table2 = new PdfPTable(1);
            table2.DefaultCell.Border = Rectangle.NO_BORDER;
            table2.WidthPercentage = 100;
            table2.SpacingAfter = 10.5f;

            PdfPCell cellTAmount = new PdfPCell();
            cellTAmount.Border = Rectangle.NO_BORDER;
            cellTAmount.AddElement(new Paragraph("•	Total Amount Paid: " + response.totalAmountPaid, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellPayR = new PdfPCell();
            cellPayR.Border = Rectangle.NO_BORDER;
            cellPayR.AddElement(new Paragraph("• Payment Record: ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            table2.AddCell(cellTAmount);
            table2.AddCell(cellPayR);
            doc.Add(table2);



            //PdfPTable table3 = new PdfPTable(2);
            //float[] headers = { 10, 80 }; //Header Widths  
            //table3.SetWidths(headers); //Set the pdf headers  
            //table3.WidthPercentage = 100; //Set the PDF File witdh percentage  
            //table3.SpacingAfter = 10.5f;

            PdfPTable table4 = new PdfPTable(3);
            float[] headers4 = { 40, 30, 30 }; //Header Widths  
            table4.SetWidths(headers4); //Set the pdf headers  
            table4.WidthPercentage = 100;
            table4.SpacingAfter = 10.5f;
            AddCellToHeader(table4, "Payment Method", 4, 1);
            AddCellToHeader(table4, "Payment Date", 4, 2);
            AddCellToHeader(table4, "Amount", 4, 3);

            if (response.coursePayment.Count() > 0)
            {
                foreach (var item in response.coursePayment)
                {
                    AddCellToBody(table4, item.paymentMethod.ToString(), 4, 1);
                    AddCellToBody(table4, item.paymentDate.ToString(), 4, 2);
                    AddCellToBody(table4, "₦ " + item.amountPaid.ToString(), 4, 3);
                }
            }

            //table3.AddCell(new PdfPCell(new Phrase("", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.White))))
            //{
            //    Border = Rectangle.NO_BORDER,
            //});
            //table3.AddCell(table4);
            doc.Add(table4);

            Paragraph para7 = new Paragraph("Important Dates:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para7.Alignment = Element.ALIGN_LEFT;
            para7.SpacingAfter = 8.5f;
            doc.Add(para7);

            PdfPTable table5 = new PdfPTable(1);
            table5.DefaultCell.Border = Rectangle.NO_BORDER;
            table5.WidthPercentage = 100;
            table5.SpacingAfter = 8.5f;


            PdfPCell cellOrienta = new PdfPCell();
            cellOrienta.Border = Rectangle.NO_BORDER;
            cellOrienta.AddElement(new Paragraph("• Orientation Session: (Orientation Date and Time)", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table5.AddCell(cellOrienta);
            doc.Add(table5);

            var phrase1 = new Phrase("Login Information: ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            var phrase2 = new Phrase("You can access the Learning Management System (LMS) using the following credentials:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));

            var paragr1 = new Paragraph();
            paragr1.Alignment = Element.ALIGN_LEFT;
            paragr1.SpacingAfter = 8.5f;
            paragr1.Add(phrase1);
            paragr1.Add(phrase2);
            doc.Add(paragr1);

            PdfPTable table6 = new PdfPTable(1);
            table6.DefaultCell.Border = Rectangle.NO_BORDER;
            table6.WidthPercentage = 100;
            table6.SpacingAfter = 8.5f;

            PdfPCell cellurl = new PdfPCell();
            cellurl.Border = Rectangle.NO_BORDER;
            cellurl.AddElement(new Paragraph("• LMS URL: " + response.LMSUrl, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellusername = new PdfPCell();
            cellusername.Border = Rectangle.NO_BORDER;
            cellusername.AddElement(new Paragraph("• Username: " + response.LMSUsername, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellpassword = new PdfPCell();
            cellpassword.Border = Rectangle.NO_BORDER;
            cellpassword.AddElement(new Paragraph("•	Temporary Password: " + response.LMSPassword, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table6.AddCell(cellurl);
            table6.AddCell(cellusername);
            table6.AddCell(cellpassword);
            doc.Add(table6);

            Paragraph para8 = new Paragraph("You will be able to access your course materials, assignments, and other resources through our Learning Management System (LMS) using the provided login credentials. For security reasons, please change your password immediately upon logging in to the LMS.", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para8.Alignment = Element.ALIGN_LEFT;
            para8.SpacingAfter = 8.5f;
            doc.Add(para8);

            var phrase3 = new Phrase("Support and Contact Information: ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            var phrase4 = new Phrase("If you have any questions, encounter issues, or need assistance, please do not hesitate to contact our Student Support Team:", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));

            var paragr3 = new Paragraph();
            paragr3.Alignment = Element.ALIGN_LEFT;
            paragr3.Add(phrase3);
            paragr3.Add(phrase4);
            doc.Add(paragr3);

            PdfPTable table7 = new PdfPTable(1);
            table7.DefaultCell.Border = Rectangle.NO_BORDER;
            table7.WidthPercentage = 100;
            table7.SpacingAfter = 10.5f;

            PdfPCell cellemail = new PdfPCell();
            cellemail.Border = Rectangle.NO_BORDER;
            cellemail.AddElement(new Paragraph("• Email: " + response.supportEmail, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            PdfPCell cellphone = new PdfPCell();
            cellphone.Border = Rectangle.NO_BORDER;
            cellphone.AddElement(new Paragraph("• Phone: " + response.supportPhone, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table7.AddCell(cellemail);
            table7.AddCell(cellphone);
            doc.Add(table7);

            Paragraph para9 = new Paragraph("Thank you for choosing EDUREX Academy for your educational journey. We wish you all the best in your studies.", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para9.Alignment = Element.ALIGN_LEFT;
            para9.SpacingAfter = 15.5f;
            doc.Add(para9);

            Paragraph para10 = new Paragraph("Warm regards,", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            para10.Alignment = Element.ALIGN_LEFT;
            para10.SpacingAfter = 10.5f;
            doc.Add(para10);

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            //_notyfService.Success("Downloaded", 5);

            return File(workStream, "application/pdf", strPDFFileName);
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
        public async Task<IActionResult> RegisterNew()
        {
           // var response = await _projectServices.GetRegisterNew();
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewProgram(DashboardVM dto)
        {
            var email = GeneralClass.email;
            var response = await _projectServices.NewProgram(dto, email);
            var dashboardRecord = new DashboardVM();
            return View();
            //if (dto.SourceView == "Dashboard")
            //{
            //    var record = await _projectServices.DashboardRe(GeneralClass.email);
            //    if (record != null)
            //    {
            //        dashboardRecord.fullName = record.fullName;
            //        dashboardRecord.programCategpryName = record.programCategpryName;
            //        dashboardRecord.PaymentRecord = record.PaymentRecord;
            //        dashboardRecord.SubjectRecord = record.SubjectRecord;
            //        dashboardRecord.ChoicesRecord = record.ChoicesRecord;
            //        dashboardRecord.ProgramCategorys = record.ProgramCategorys;
            //        dashboardRecord.programListz = await _projectServices.GetPrograms();
            //        dashboardRecord.rCode = record.rCode;
            //    }
            //    return View(dto.SourceView, dashboardRecord);
            //}
            //else
            //{
            //    var response2 = await _projectServices.UserPrograms(email);
            //    var programs = await _projectServices.GetPrograms();
            //    var result = new DashboardVM();
            //    result.ProgramCategorys = response2;
            //    result.SourceView = "AllPrograms";
            //    result.programListz = programs;
            //    return View(dto.SourceView, result);
            //}
        }
        public async Task<IActionResult> AllPrograms()
        {
            //var email = GeneralClass.email;
            GeneralClass.source = "AllPrograms";
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            // var response = await _projectServices.UserPrograms(email);
            var programs = await _projectServices.GetPrograms();
            var result = new DashboardVM();
            //result.ProgramCategorys = response;
            //result.SourceView = "AllPrograms";
            //result.programListz = programs;
            return View(result);
        }
        public async Task<IActionResult> ReferralUsage()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var response = await _projectServices.UserReferralCodeUsage(user.Email);
            return View(response);
        }
        public async Task<IActionResult> DiscountUsage(string code)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var res = await _projectServices.DiscountHistory(code);
            return View(res);
        }
        public async Task<IActionResult> Payment()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var response = await _projectServices.UserPaymentHistories(user.Id);
            var result = new UserPaymentsVM();
            result.UserPaymentList = response;
            return View(result);
        }
        public async Task<IActionResult> EachPayment(string paymentRef)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var response = await _projectServices.EachPaymentHistories(user.Id, paymentRef);
            var result = new UserPaymentsVM();
            result.UserPaymentList = response;
            return View(result);
        }
        public async Task<IActionResult> PaymentReceipt(string paymentRef)
        {
            return View();
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //if (user == null)
            //{
            //    _notyfService.Error("Invalid user", 10);

            //    return RedirectToAction(nameof(Login));
            //}
            //var response = await _projectServices.GenerateInvoice(user.Id, paymentRef);
           
            //return View(response);
        }
        //public FileResult DownloadReceipt(string paymentRef)
        //{
            
        //   var a= Pdf(paymentRef);

        //    _notyfService.Success("Receipt generated successfully", 10);

        //    return a;
        //}

        public FileResult DownloadReceipt(string paymentRef)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            
            var user = _userManager.Users.Where(x=>x.UserName == sessionValue).FirstOrDefault();

            var response = _projectServices.GenerateInvoice(user.Id, paymentRef);

            Document doc = new Document(PageSize.A4);
            MemoryStream workStream = new MemoryStream();
            string strPDFFileName = string.Format(response.ReceiptNum + ".pdf");

            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();


            var logo = iTextSharp.text.Image.GetInstance("https://my.edurex.academy/assets/images/EdurexAcademy.png");
            logo.ScaleAbsoluteHeight(120);
            logo.ScaleAbsoluteWidth(150);

            PdfPTable table1 = new PdfPTable(4);
            table1.DefaultCell.Border = Rectangle.NO_BORDER;
            table1.WidthPercentage = 100;

            //var titleFont = new Font(Font.FontFamily.UNDEFINED, 24);
            //var subTitleFont = new Font(Font.FontFamily.UNDEFINED, 16);
            //var subTitleColor = new BaseColor(35, 31, 32);

            PdfPCell cellLogo = new PdfPCell();
            cellLogo.Border = Rectangle.NO_BORDER;
            cellLogo.AddElement(logo);
            cellLogo.Colspan = 3;

            PdfPCell cellInvoice = new PdfPCell();
            cellInvoice.Border = Rectangle.NO_BORDER;
            cellInvoice.VerticalAlignment = Element.ALIGN_RIGHT;
            Paragraph Invoice = new Paragraph("RECEIPT", FontFactory.GetFont("Arial", 15, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            Invoice.Alignment = Element.ALIGN_RIGHT;
            cellInvoice.AddElement(Invoice);
            Paragraph ReceiptN = new Paragraph("# " + response.ReceiptNum, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            ReceiptN.Alignment = Element.ALIGN_RIGHT;
            cellInvoice.AddElement(ReceiptN);
            
            if (response.CoursePaymentStatus == UserProgramPaymentStatusEnums.Paid)
            {
                Paragraph Paid = new Paragraph("PAID", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Green)));
                Paid.Alignment = Element.ALIGN_RIGHT;
                cellInvoice.AddElement(Paid);
            }
            else
            {
                Paragraph Deposited = new Paragraph("PARTIALLY PAID", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.OrangeRed)));
                Deposited.Alignment = Element.ALIGN_RIGHT;
                cellInvoice.AddElement(Deposited);
            }
            table1.AddCell(cellLogo);
            table1.AddCell(cellInvoice);
            table1.SpacingAfter = 12.5f;

            PdfPTable table2 = new PdfPTable(2);
            table2.DefaultCell.Border = Rectangle.NO_BORDER;
            table2.WidthPercentage = 100;

            PdfPCell cellEdurexInfo = new PdfPCell();
            cellEdurexInfo.Border = Rectangle.NO_BORDER;
            cellEdurexInfo.VerticalAlignment = Element.ALIGN_LEFT;
            cellEdurexInfo.AddElement(new Paragraph("Edurex ACADEMY", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            cellEdurexInfo.AddElement(new Paragraph("20 Awolowo Avenue,", FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            cellEdurexInfo.AddElement(new Paragraph("Bodija, Ibadan.", FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            cellEdurexInfo.AddElement(new Paragraph("Oyo.", FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            cellEdurexInfo.AddElement(new Paragraph("Nigeria", FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            PdfPCell cellPayeeInfo = new PdfPCell();
            cellPayeeInfo.Border = Rectangle.NO_BORDER;
            var city = ""; var state = ""; var country = "";  
            if(response.users != null)
            {
                if(response.users.CityId > 0)
                {
                    var res = _context.Cities.Include(x => x.State).ThenInclude(x => x.Country).Where(x => x.Id == response.users.CityId).Select(x => new { cityValue = x.Name, stateValue = x.State.Name, countryValue = x.State.Country.Name }).FirstOrDefault();
                    city = res.cityValue;
                    state = res.stateValue;
                    country = res.countryValue;
                }
            }
            cellPayeeInfo.VerticalAlignment = Element.ALIGN_RIGHT;
            Paragraph Payee = new Paragraph("Payment Received From: " + response.users.FirstName + " " + response.users.LastName, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            Payee.Alignment = Element.ALIGN_RIGHT;
            cellPayeeInfo.AddElement(Payee);

            //cellPayeeInfo.AddElement(new Paragraph(response.users.FirstName + " " + response.users.LastName, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));
            Paragraph Address = new Paragraph(response.users.Address, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            Address.Alignment = Element.ALIGN_RIGHT;
            cellPayeeInfo.AddElement(Address);

            Paragraph CityState = new Paragraph(city + ", " + state, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            CityState.Alignment = Element.ALIGN_RIGHT;
            cellPayeeInfo.AddElement(CityState);

            Paragraph Country = new Paragraph(country, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            Country.Alignment = Element.ALIGN_RIGHT;
            cellPayeeInfo.AddElement(Country);

            table2.AddCell(cellEdurexInfo);
            table2.AddCell(cellPayeeInfo);
            table2.SpacingAfter = 8.5f;

            PdfPTable table3 = new PdfPTable(1);
            table3.DefaultCell.Border = Rectangle.NO_BORDER;
            table3.WidthPercentage = 100;

            PdfPCell cellInvoiceInfo = new PdfPCell();
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            cellInvoiceInfo.VerticalAlignment = Element.ALIGN_RIGHT;
            Paragraph InvoiceDate = new Paragraph("Receipt Date: " + response.paymentDate, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            InvoiceDate.Alignment = Element.ALIGN_RIGHT;
            cellInvoiceInfo.AddElement(InvoiceDate);

            table3.AddCell(cellInvoiceInfo);
            table3.SpacingAfter = 8.5f;

            PdfPTable table4 = new PdfPTable(3);
            float[] headers = { 20, 70, 30 }; //Header Widths  
            table4.SetWidths(headers); //Set the pdf headers  
            table4.WidthPercentage = 100; //Set the PDF File witdh percentage  
            table4.HeaderRows = 1;
            table4.SpacingAfter = 10.5f;

            var count = 1;
            AddCellToHeader(table4, "#", 1, 1);
            AddCellToHeader(table4, "Description", 1, 2);
            AddCellToHeader(table4, "Amount Paid", 1, 3);

            PaymentMethodEnums paymentMethod = new PaymentMethodEnums();
            if(response.PaymentRecord.Count()>0)
            {
                foreach (var item in response.PaymentRecord)
                {
                    paymentMethod = item.PaymentMethod;
                    //Add body  
                    AddCellToBody(table4, count.ToString(), 1, 1);
                    AddCellToBody(table4, item.paymentForName.ToString(), 1, 2);
                    AddCellToBody(table4, "₦ " + item.Amount.ToString(), 1, 3);
                    count++;
                }
            }

            PdfPTable table5 = new PdfPTable(3);
            table5.DefaultCell.Border = Rectangle.NO_BORDER;
            table5.WidthPercentage = 100;
            float[] headers5 = { 20, 90, 30 }; //Header Widths
            table5.SetWidths(headers5);

            PdfPCell cellSubTotal = new PdfPCell();
            cellSubTotal.VerticalAlignment = Element.ALIGN_RIGHT;
            cellSubTotal.Border = Rectangle.NO_BORDER;
            cellSubTotal.Colspan = 2;
            var phrase1 = new Phrase("Sub Total: ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));

            var paragr1 = new Paragraph();
            paragr1.Alignment = Element.ALIGN_RIGHT;
            paragr1.Add(phrase1);
            // paragr.Add(phrase2);

            cellSubTotal.AddElement(paragr1);

            PdfPCell cellSubTotalAmount = new PdfPCell();
            cellSubTotalAmount.VerticalAlignment = Element.ALIGN_RIGHT;
            cellSubTotalAmount.Border = Rectangle.NO_BORDER;
            var paragr2 = new Paragraph();
            paragr2.Alignment = Element.ALIGN_RIGHT;
            var phrase2 = new Phrase(response.totalPayment, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
            paragr2.Add(phrase2);
            cellSubTotalAmount.AddElement(paragr2);


            table5.AddCell(cellSubTotal);
            table5.AddCell(cellSubTotalAmount);
            table5.SpacingAfter = 8.5f;

            PdfPTable table6 = new PdfPTable(1);
            table6.DefaultCell.Border = Rectangle.NO_BORDER;
            table6.WidthPercentage = 100;
            PdfPCell cellTransaction = new PdfPCell();
            cellTransaction.Border = Rectangle.NO_BORDER;
            cellTransaction.VerticalAlignment = Element.ALIGN_LEFT;
            cellTransaction.AddElement(new Paragraph("Transactions: ", FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table6.AddCell(cellTransaction);
            table6.SpacingAfter = 8.5f;

            PdfPTable table7 = new PdfPTable(4);
            float[] headers7 = { 20, 50, 50, 30 }; //Header Widths  
            table7.SetWidths(headers7); //Set the pdf headers  
            table7.WidthPercentage = 100; //Set the PDF File witdh percentage  
            table7.HeaderRows = 1;
            table7.SpacingAfter = 100.5f;

            AddCellToHeader(table7, "#", 2, 1);
            AddCellToHeader(table7, "Payment Mode", 2, 2);
            AddCellToHeader(table7, "Date", 2, 3);
            AddCellToHeader(table7, "Amount", 2, 4);

            AddCellToBody(table7, "1", 2, 1);
            AddCellToBody(table7, paymentMethod + "", 2, 2);
            AddCellToBody(table7, response.paymentDate.ToString(), 2, 3);
            AddCellToBody(table7, response.totalPayment.ToString(), 2, 4);

            PdfPTable table8 = new PdfPTable(1);
            table8.DefaultCell.Border = Rectangle.NO_BORDER;
            table8.WidthPercentage = 100;
            PdfPCell cellSignature = new PdfPCell();
            cellSignature.Border = Rectangle.NO_BORDER;
            cellSignature.VerticalAlignment = Element.ALIGN_LEFT;
            cellSignature.AddElement(new Paragraph("Authorized Signature_______________________________", FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))));

            table8.AddCell(cellSignature);
            table8.SpacingAfter = 8.5f;

            doc.Add(table1);
            doc.Add(table2);
            doc.Add(table3);
            doc.Add(table4);
            doc.Add(table5);
            doc.Add(table6);
            doc.Add(table7);
            doc.Add(table8);

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;
            return File(workStream, "application/pdf", strPDFFileName);

        }
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText, int table, int Column)
        {
            if (table == 1)
            {
                if (Column == 3)
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.White))))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 8,
                        //PaddingBottom = 8,
                        BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.Black),
                        Border = Rectangle.NO_BORDER,
                        //PaddingLeft = 30,
                    });
                }
                else
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.White))))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 8,
                        BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.Black),
                        Border = Rectangle.NO_BORDER
                    });
                }

            }
            else if (table == 2)
            {
                if (Column == 4)
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 8,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = 2
                    });
                }
                else
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 8,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = 2
                    });
                }

            }
            else
            {
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 8,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                    Border = 2
                });
            }
        }
        private static void AddCellToBody(PdfPTable tableLayout, string cellText, int table, int column)
        {
            if (table == 1)
            {
                if (column == 3)
                {
                    var phrase1 = new Paragraph(cellText, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
                    phrase1.Alignment = Element.ALIGN_RIGHT;
                    //PdfPCell cellAmount = new PdfPCell();
                    //cellAmount.AddElement(phrase1);

                    tableLayout.AddCell(new PdfPCell(phrase1)
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 6,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = Rectangle.NO_BORDER

                    });
                }
                else
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 6,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = Rectangle.NO_BORDER
                    });
                }

            }
            else if(table == 2)
            {
                if (column == 4)
                {
                    var phrase1 = new Paragraph(cellText, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black)));
                    phrase1.Alignment = Element.ALIGN_RIGHT;
                    tableLayout.AddCell(new PdfPCell(phrase1)
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 6,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = Rectangle.NO_BORDER

                    });
                }
                else
                {
                    tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 6,
                        BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                        Border = Rectangle.NO_BORDER
                    });
                }
            }
            else
            {
                tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black))))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 6,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255),
                    Border = Rectangle.NO_BORDER
                });
            }
        }
        public async Task<IActionResult> GetRequest()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var res = await _projectServices.GetRequest(user.Id);
            return View(res);
        }
        public async Task<IActionResult> NewRequest()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var req = new UserRequest();
            return View(req);
        }

        [HttpPost]
        public async Task<IActionResult> NewRequest(UserRequest dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var res = await _projectServices.SendRequest(dto,user.Id);
            return View("GetRequest", res);
        }
        public async Task<IActionResult> SingleRequest(int reqId)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var res = await _projectServices.SingleRequest(reqId, user.Id);
            return View(res);
        }
        public async Task<IActionResult> ChangeStartDate()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStartDate(StudentDashboardVM dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }

            dto.ChangeCDateDTO.paymentDTO.UserId = user.Id;
            dto.ChangeCDateDTO.userId = user.Id;
            var response = await _projectServices.ChangeCourseDate(dto.ChangeCDateDTO, dto.SourceView);
            if (response != null)
            {
                if (response.checkOutURL.Contains("htt"))
                {
                    GeneralClass.pRef = response.paymentRef;
                    return new RedirectResult(response.checkOutURL, true);
                }

               // _notyfService.Success("Saved Successfully", 5);
                return RedirectToAction(dto.SourceView, "Home", new { area = "" });
               // return RedirectToAction(nameof(dto.SourceView));
            }
            return RedirectToAction(dto.SourceView, "Home", new { area = "" });

        }


        public async Task<IActionResult> Certificates()
        {
            return View();
        }
        
        [HttpPost("CardMakePayment")]
        public async Task<IActionResult> CardMakePayment(DashboardVM dto)
        {
            return View();
            //dto.paymentDTO.PaymentMethod = PaymentMethodEnums.Card;

            //var result = await MakePayment(dto);
            //if(result !="")
            //{
            //    return new RedirectResult(result, true);
            //}
            //return View(dto.SourceView);
        }
        [HttpPost("TransferMakePayment")]
        public async Task<IActionResult> TransferMakePayment(DashboardVM dto)
        {
            return View();
            //dto.paymentDTO.PaymentMethod = PaymentMethodEnums.AccountTransfer;

            //var result = await MakePayment(dto);
            //if (result != "")
            //{
            //    return new RedirectResult(result, true);
            //}
            //return View(dto.SourceView);
        }
        [HttpPost("BankConnectMakePayment")]
        public async Task<IActionResult> BankConnectMakePayment(DashboardVM dto)
        {
            return View();
            //dto.paymentDTO.PaymentMethod = PaymentMethodEnums.BankConnect;

            //var result = await MakePayment(dto);
            //if (result != "")
            //{
            //    return new RedirectResult(result, true);
            //}
            //return View(dto.SourceView);
        }
        public async Task<IActionResult> MakePayment()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MakePayment(StudentDashboardVM dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            dto.ChangeCDateDTO.paymentDTO.UserId = user.Id;
            dto.ChangeCDateDTO.userId = user.Id;
            dto.ChangeCDateDTO.paymentDTO.PaymentFor = paymentForEnums.Course;

            var paymentResponse = new PaymentInitialiseResponse();

            PaymentMethodEnums paymentM = new PaymentMethodEnums();
            var paymentList = new List<PaymentDTO2>();
            string subRef = $"{DateTime.Now.Ticks}{(int)paymentM}";
            string discription = "";
            //Get if the person is a new user
            //-------------------------------
            float discount = 0;
            var referral = await _context.UserReferred.Include(x => x.Referral).Where(x => x.ReferredUserId == user.Id).FirstOrDefaultAsync();
            if (referral != null)
            {
                var courseDate = await _context.UserCourses.Include(x => x.CoursePriceOption).ThenInclude(x => x.Course).Where(x => x.Id == dto.ChangeCDateDTO.paymentDTO.UserPaymentForId).Select(x => new { coursePrice = x.CoursePriceOption.Amount }).FirstOrDefaultAsync();

                discription = "Referral";

                var role = await _roleManager.FindByIdAsync(referral.Referral.DefaultRole);
                if (role.Name == "Staff")
                {
                    discount = (7 * courseDate.coursePrice) / 100;
                    // perAmount = dto.userCourseOption.AmountPaid;
                    referral.ReferralDiscount = 3;
                    referral.ReferredDiscount = 7;
                }
                else if (role.Name == "Freelance")
                {
                    discount = (5 * courseDate.coursePrice) / 100;
                    //perAmount = dto.userCourseOption.AmountPaid;
                    referral.ReferralDiscount = 5;
                    referral.ReferredDiscount = 5;
                }

               // refer = referral.Id;

                var refePH = new UserReferralPaymentHistory
                {
                    PaymentRef = subRef,
                    UserReferId = referral.Id,
                    Amount = (float)dto.ChangeCDateDTO.paymentDTO.Amount,
                    UserCourseId = dto.ChangeCDateDTO.paymentDTO.UserPaymentForId
                };

                await _context.UserReferralPaymentHistory.AddAsync(refePH);
                await _context.SaveChangesAsync();
            }
            
            paymentList.Add(dto.ChangeCDateDTO.paymentDTO);
            if (dto.ChangeCDateDTO.paymentDTO.PaymentMethod.ToLower() == "card")
            {
                paymentM = PaymentMethodEnums.Card;                
                foreach(var item in paymentList)
                {
                    item.paymentRef = subRef;
                    item.discountAm = discount;
                }

                paymentResponse = await _projectServices.InitializeCardPayment2(paymentList, paymentM, "Dashboard");
            }
            else if (dto.ChangeCDateDTO.paymentDTO.PaymentMethod.ToLower() == "offline")
            {
                paymentM = PaymentMethodEnums.Offline;
                subRef = dto.ChangeCDateDTO.paymentDTO.paymentRef;
                foreach (var item in paymentList)
                {
                    item.paymentRef = subRef;
                    item.discountAm = discount;
                }
                paymentResponse = await _projectServices.InitializeOfflinePayment2(paymentList, "Dashboard");
            }

            //if (dto.paymentDTO.PaymentMethod == PaymentMethodEnums.Card)
            //{
            //    paymentResponse = await _projectServices.InitializeCardPayment(dto.paymentDTO, PaymentMethodEnums.Card, dto.SourceView);
            //}
            //else if (dto.paymentDTO.PaymentMethod == PaymentMethodEnums.AccountTransfer)
            //{
            //    paymentResponse = await _projectServices.InitializeAccountTransferPayment(dto.paymentDTO, PaymentMethodEnums.AccountTransfer, dto.SourceView);
            //}
            //else if (dto.paymentDTO.PaymentMethod == PaymentMethodEnums.BankConnect)
            //{
            //    paymentResponse = await _projectServices.InitializeBankCOnnectPayment(dto.paymentDTO, dto.SourceView);
            //}

            GeneralClass.pRef = "";
            if (paymentResponse.checkOutURL.Contains("htt"))
            {
                GeneralClass.pRef = paymentResponse.paymentRef;
               return new RedirectResult(paymentResponse.checkOutURL, true);
                
            }
            return RedirectToAction(dto.SourceView, "Home", new { area = "" });

        }
        public async Task<IActionResult> Logout()
        {
            var res =await  _projectServices.Logout();
            HttpContext.Session.Remove(SessionUsername);

            return RedirectToAction("Login", "Home", new { area = "" });


        }
        public IActionResult Login()
        {
            HttpContext.Session.Remove(SessionUsername);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var userLogin = await _projectServices.Login(dto);
            if(userLogin.Item2 == "Successful")
            {
                HttpContext.Session.SetString(SessionUsername, userLogin.Item1.UserName.ToString());

                //_notyfService.Success("Welcome back, " + userLogin.Item1.Email, 10);
                //GeneralClass.email = userLogin.Item1.Email;

                return RedirectToAction("Dashboard", "Home", new { area = "" });

            }
            else if(userLogin.Item2 == "Incomplete")
            {
                HttpContext.Session.SetString(SessionUsername, userLogin.Item1.UserName.ToString());

                _notyfService.Success("Welcome back, " + userLogin.Item1.Email, 10);

                return RedirectToAction("Register2", "Enrollment", new { area = "" });
            }
            else if (userLogin.Item2 == "Not Found")
            {
                _notyfService.Error("Invalid user.", 10);

                return RedirectToAction("Register", "Enrollment", new { area = "" });
            }
            else
            {
                _notyfService.Error(userLogin.Item2, 10);

                return View();
            }
                     
        }
        [HttpGet]
        public async Task<IActionResult> StudentDashboard()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            GeneralClass.source = "StudentDashboard";
            //GeneralClass.email = user.Email;

            var dashboardRecord = new StudentDashboardVM();
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var pRef = GeneralClass.pRef;
            var userId = user.Id;
            IPAddress remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            string result = "";
            if (remoteIpAddress != null)
            {
                // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
                // This usually only happens when the browser is on the same machine as the server.
                if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
            .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                }
                result = remoteIpAddress.ToString();
            }
            if (pRef != null && pRef != "")
            {
                var response2 = await _projectServices.QueryBankCOnnectPayment(pRef);
                var record = await _projectServices.DashboardRe(userId, result);
                if (record != null)
                {
                    dashboardRecord.fullName = record.fullName;
                    dashboardRecord.studentNumber = record.studentNumber;
                    dashboardRecord.PaymentRecord = record.PaymentRecord;
                    dashboardRecord.UserCertificationsList = record.UserCertificationsList;
                    dashboardRecord.UserCoursesVM = record.UserCoursesVM;
                    dashboardRecord.UserDataList = record.UserDataList;
                    dashboardRecord.UserDevicesList = record.UserDevicesList;
                    dashboardRecord.programListz = record.programListz;

                }
                return View(dashboardRecord);

            }
            else
            {
                var record = await _projectServices.DashboardRe(userId, result);
                if (record != null)
                {
                    dashboardRecord.fullName = record.fullName;
                    dashboardRecord.studentNumber = record.studentNumber;
                    dashboardRecord.PaymentRecord = record.PaymentRecord;
                    dashboardRecord.UserCertificationsList = record.UserCertificationsList;
                    dashboardRecord.UserCoursesVM = record.UserCoursesVM;
                    dashboardRecord.UserDataList = record.UserDataList;
                    dashboardRecord.UserDevicesList = record.UserDevicesList;
                    dashboardRecord.programListz = record.programListz;

                }
                return View(dashboardRecord);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            GeneralClass.source = "Dashboard";
            GeneralClass.email = user.Email;

            var dashboardRecord = new StudentDashboardVM();
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var pRef = GeneralClass.pRef;
            var userId = user.Id;
            IPAddress remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            string result = "";
            if (remoteIpAddress != null)
            {
                // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
                // This usually only happens when the browser is on the same machine as the server.
                if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
            .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                }
                result = remoteIpAddress.ToString();
            }
            if (pRef != null && pRef != "")
            {
                var response2 = await _projectServices.QueryBankCOnnectPayment(pRef);
                var record = await _projectServices.DashboardRe(userId, result);
                if (record != null)
                {
                    dashboardRecord.fullName = record.fullName;
                    dashboardRecord.studentNumber  = record.studentNumber;
                    dashboardRecord.PaymentRecord = record.PaymentRecord;
                    dashboardRecord.UserCertificationsList = record.UserCertificationsList;
                    dashboardRecord.UserCoursesVM = record.UserCoursesVM;
                    dashboardRecord.UserDataList  = record.UserDataList;
                    dashboardRecord.UserDevicesList  = record.UserDevicesList;
                    dashboardRecord.programListz  = record.programListz;

                }
                return View(dashboardRecord);
                
            }
            else
            {
                var record = await _projectServices.DashboardRe(userId, result);
                if (record != null)
                {
                    dashboardRecord.fullName = record.fullName;
                    dashboardRecord.studentNumber = record.studentNumber;
                    dashboardRecord.PaymentRecord = record.PaymentRecord;
                    dashboardRecord.UserCertificationsList = record.UserCertificationsList;
                    dashboardRecord.UserCoursesVM = record.UserCoursesVM;
                    dashboardRecord.UserDataList = record.UserDataList;
                    dashboardRecord.UserDevicesList = record.UserDevicesList;
                    dashboardRecord.programListz = record.programListz;

                }
                return View(dashboardRecord);
            }
        }
        public async  Task<IActionResult> Courses()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var pRef = GeneralClass.pRef;
            if (pRef != null && pRef != "")
            {
                var response2 = await _projectServices.QueryBankCOnnectPayment(pRef);
            }
            var courses = await _projectServices.StudentCourses(user.Id);
            var st = new UserCoursesVM()
            {
                UserCourseList = courses
            };
            var vm = new StudentDashboardVM
            {
               UserCoursesVM = st
            };
            return View(vm);
        }
        public async Task<IActionResult> SingleCourse(int id)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var record = await _projectServices.SingleCourse(id, user.Id);
            return View(record);
        }
        public async Task<IActionResult> Certifications()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var courses = await _projectServices.StudentCertifications(user.Id);
            var vm = new UserCertificationsVM
            {
                UserCertificationList = courses
            };
            return View(vm);
        }
        public async Task<IActionResult> SubscriptionDevices()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var vm = await _projectServices.StudentDataDevices(user.Id);
            
            return View(vm);
        }
        public async Task<IActionResult> GetProfile()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var courses = await _projectServices.GetProfile(user.Id);

            var vm = new UserProfileVM
            {
                User = courses
            };
            return View(vm);
        }
        public async Task<IActionResult> GetWallet()
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var courses = await _projectServices.GetWallet(user.Id);

            return View(courses);
        }
        public IActionResult OnlyCertifications()
        {
            GeneralClass.certOnly = true;
            return RedirectToAction("Register2", "Enrollment", new { area = "" });
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult bulkPO()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {            
            var response = await _projectServices.GetRegister();            
            return View(response);            
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> ReferRegister(string code)
        {
            var response = await _projectServices.GetRegister();
            if (code != null && code != "")
            {
                response.refCode = code;
            }
            return View(response);
        }
        public async Task<IActionResult> QueryPaymentStatus(string paymentReference)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var response = await _projectServices.QueryBankCOnnectPayment(paymentReference);
            return View(response);
        }

        //[HttpPost]
        //public async Task<IActionResult> Register(RegisterVM dto)
        //{
        //    var paymentResponse = await _projectServices.RegisterUser(dto.registerz);
        //    GeneralClass.pRef = "";
        //    if (paymentResponse.checkOutURL.Contains("htt"))
        //    {
        //        GeneralClass.pRef = paymentResponse.paymentRef;
        //        GeneralClass.email = dto.registerz.Email;

        //        return new RedirectResult(paymentResponse.checkOutURL, true);
        //    }

        //    var response = await _projectServices.GetRegister();
        //    return View(response);
        //}
        [HttpPost("RegisterCard")]
        public async Task<IActionResult> RegisterCard(RegisterVM dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            dto.registerz.PaymentMethod = 1;
            //var pDeposit = await _projectServices.GetPriceByProgramId(dto.registerz.ProgramId);
            //dto.registerz.paymentDeposit = pDeposit.Split(',')[1];
            if (!ModelState.IsValid)
            {
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
            var paymentResponse = await _projectServices.RegisterUser(dto.registerz);
            GeneralClass.pRef = "";
            if (paymentResponse.checkOutURL != null && paymentResponse.checkOutURL.Contains("htt"))
            {
                GeneralClass.pRef = paymentResponse.paymentRef;
                GeneralClass.email = dto.registerz.Email;
                GeneralClass.FullName = $"{dto.registerz.FirstName} {dto.registerz.LastName}";

                return new RedirectResult(paymentResponse.checkOutURL, true);
            }

            var response = await _projectServices.GetRegister();
            return View("Register",response);
        }
        [HttpPost("RegisterTransfer")]
        public async Task<IActionResult> RegisterTransfer(RegisterVM dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            dto.registerz.PaymentMethod = 2;
            //var pDeposit = await _projectServices.GetPriceByProgramId(dto.registerz.ProgramId);
            //dto.registerz.paymentDeposit = decimal.Parse(pDeposit.Split(',')[1]);
            if (!ModelState.IsValid)
            {
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
           
            var paymentResponse = await _projectServices.RegisterUser(dto.registerz);
            GeneralClass.pRef = "";
            if (paymentResponse.checkOutURL != null && paymentResponse.checkOutURL.Contains("htt"))
            {
                GeneralClass.pRef = paymentResponse.paymentRef;
                GeneralClass.email = dto.registerz.Email;
                GeneralClass.FullName = $"{dto.registerz.FirstName} {dto.registerz.LastName}";


                return new RedirectResult(paymentResponse.checkOutURL, true);
            }

            var response = await _projectServices.GetRegister();
            return View("Register", response);
        }
        [HttpPost("RegisterBankConnect")]
        public async Task<IActionResult> RegisterBankConnect(RegisterVM dto)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            dto.registerz.PaymentMethod = 3;
            //var pDeposit = await _projectServices.GetPriceByProgramId(dto.registerz.ProgramId);
            //dto.registerz.paymentDeposit = decimal.Parse(pDeposit.Split(',')[1]);
            if (!ModelState.IsValid)
            {
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
            var paymentResponse = await _projectServices.RegisterUser(dto.registerz);
            GeneralClass.pRef = "";
            if (paymentResponse.checkOutURL != null && paymentResponse.checkOutURL.Contains("htt"))
            {
                GeneralClass.pRef = paymentResponse.paymentRef;
                GeneralClass.email = dto.registerz.Email;
                GeneralClass.FullName = $"{dto.registerz.FirstName} {dto.registerz.LastName}";

                return new RedirectResult(paymentResponse.checkOutURL, true);
            }

            var response = await _projectServices.GetRegister();
            return View("Register", response);
        }
        [HttpPost("RegisterOffline")]
        public async Task<IActionResult> RegisterOffline(RegisterVM dto)
        {
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //if (user == null)
            //{
            //    _notyfService.Error("Invalid user", 10);

            //    return RedirectToAction("Login", "Home", new { area = "" });

            //}
            dto.registerz.PaymentMethod = 5;

            if (!ModelState.IsValid)
            {
                var response1 = await _projectServices.GetRegister();
                return View("Register", response1);
            }
            var paymentResponse = await _projectServices.RegisterUser(dto.registerz);
            GeneralClass.pRef = "";
            if (paymentResponse.checkOutURL != null && paymentResponse.checkOutURL =="Successful")
            {
                GeneralClass.pRef = paymentResponse.paymentRef;
                GeneralClass.email = dto.registerz.Email;
                GeneralClass.FullName = $"{dto.registerz.FirstName} {dto.registerz.LastName}";

                return RedirectToAction(nameof(Dashboard));
            }

            var response = await _projectServices.GetRegister();
            return View("Register", response);
        }
        public async Task<IActionResult> GetPrograms()
        {
            var programList = await _projectServices.GetPrograms();

            programList.Insert(0, new Programs { Id = 0, Name = "Select an option" });

            return Json(new SelectList(programList, "Id", "Name"));
        }
        public async Task<IActionResult> GetProgramCategoriess(int ProgramId)
        {
            var programCatList = await _projectServices.GetProgramCatByProgramId(ProgramId);

            programCatList.Insert(0, new ProgramCategory { Id = 0, Name = "Select an option" });

            return Json(new SelectList(programCatList, "Id", "Name"));
        }
        public async Task<IActionResult> GetProgramOptions(int CategoryId)
        {
            
            var programList = await _projectServices.GetProgramOptionsByCategoryId(CategoryId);

            programList.Insert(0, new ProgramOptions { Id = 0, Name = "Select an option" });

            return Json(new SelectList(programList, "Id", "Name"));
        }
        public async Task<IActionResult> GetProgramSubjects(int OptionId)
        {
            var programList = await _projectServices.GetProgramSubjectssByOptionId(OptionId);

            //programList.Insert(0, new Subjects { Id = 0, Name = "Select an option" });

            return Json(new SelectList(programList, "Id", "Name"));
        }
        public async Task<IActionResult> GetCountries()
        {
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //if (user == null)
            //{
            //    _notyfService.Error("Invalid user", 10);

            //    return RedirectToAction("Login", "Home", new { area = "" });

            //}
            var stateList = await _projectServices.GetCountries();
            return Json(new SelectList(stateList, "Id", "Name"));
        }
        public async Task<IActionResult> statesByCountryId(int CountryId)
        {
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //if (user == null)
            //{
            //    _notyfService.Error("Invalid user", 10);

            //    return RedirectToAction("Login", "Home", new { area = "" });

            //}
            var stateList = await _projectServices.GetStatesByCountryId(CountryId);

            stateList.Insert(0, new States {  Id= 0, Name = "Select an option" });

            return Json(new SelectList(stateList, "Id", "Name"));
        }
        public async Task<string> GetCourierFeeByStateId(int StateId)
        {
            var cityList = await _projectServices.GetCourierFeeByStateId(StateId);
            return (cityList);
        }
        public async Task<IActionResult> citiesByStateId(int StateId)
        {
            
            var cityList = await _projectServices.GetCitiesByStateId(StateId);

            cityList.Insert(0, new Cities { Id = 0, Name = "Select an option" });

            return Json(new SelectList(cityList, "Id", "Name"));
        }

        //public async Task<IActionResult> streetsByCityId(int CityId)
        //{
        //    var streetList = await _projectServices.GetStreetsByCityId(CityId);

        //    streetList.Insert(0, new Utilities.Streets { streetId = 0, streetName = "Select an option" });

        //    return Json(new SelectList(streetList, "streetId", "streetName"));
        //}

        public async Task<IActionResult> GetInstitutions()
        {
            
            var institutionList = await _projectServices.GetInstitutions();

            institutionList.Insert(0, new Institutions { Id = 0, Name = "Select an option" });

            return Json(new SelectList(institutionList, "Id", "Name"));
        }

        public async Task<IActionResult> GetCoursesbyProgramCat(int CategoryId)
        {
            
            var courseList = await _projectServices.GetCoursesbyProgramCat(CategoryId);

            courseList.Insert(0, new Courses { Id = 0, Name = "Select an option" });

            return Json(new SelectList(courseList, "Id", "Name"));
        }
        public async Task<IActionResult> GetCourseOptionsDatesbyCourseId(int CourseId)
        {
            
            var CourseOptionsList = await _projectServices.GetCourseOptionsDatesbyCourseId(CourseId);

            return Json(new SelectList(CourseOptionsList, "DateRange", "DateRange"));
        }
        public async Task<IActionResult> GetCourseOptionsbyOptionDate(string OptionDate, int selectedCourseId)
        {
            var CourseOptionsList = await _projectServices.GetCourseOptionsbyOptionDate(OptionDate, selectedCourseId);

            return Json(new SelectList(CourseOptionsList, "Id", "Name"));
        }
        public async Task<IActionResult> GetCertificatesOptionsbyId(int CertId)
        {
            
            var CourseOptionsList = await _projectServices.GetCertificatesOptionsbyId(CertId);

            return Json(new SelectList(CourseOptionsList, "Id", "ExamDate"));
        }
        public async Task<string> GetInstitutionbyCourseId(int CourseId)
        {
            var CourseOptionsList = await _projectServices.GetCourseOptionsDatesbyCourseId(CourseId);
            var institutionName = "";
            if(CourseOptionsList.Count()>0)
            {
                foreach(var item in CourseOptionsList)
                {
                    institutionName = item.institutionName;
                    break;
                }
            }
            return (institutionName);
        }
        public async Task<IActionResult> GetCertificatesbyCourseId(int CourseId)
        {
            
            var CertificationsList = await _projectServices.GetCertificatesbyCourseId(CourseId);

            return Json(new SelectList(CertificationsList, "Id", "Name"));
        }
        public async Task<IActionResult> GetCertificatesbyCategoryId(int CategoryId)
        {
            
            var CertificationsList = await _projectServices.GetCertificatesbyCategoryId(CategoryId);

            return Json(new SelectList(CertificationsList, "Id", "Name"));
        }
        public async Task<IActionResult> GetCertificateType()
        {
            
            var CertificationsList = await _projectServices.GetCertificateType();

            return Json(new SelectList(CertificationsList, "Id", "CertType"));
        }
        public async Task<string> GetCertificateFeeByTypeId(int TypeId)
        {

            var TypeFee = await _projectServices.GetCertificateFeeByTypeId(TypeId);
            return (TypeFee);
        }
        public async Task<string> GetCertificationConvertedValuebyCertOptId(int CertOptId)
        {
            var TypeFee = await _projectServices.GetCertificationConvertedValuebyCertOptId(CertOptId);
            return (TypeFee);
        }
        //public async Task<string> programPriceByProgramId(string ProgramId)
        //{
        //    var programPrice = await _projectServices.GetPriceByProgramId(Convert.ToInt32(ProgramId));
        //   // var Price = programPrice.Split(',')[0];
        //    GeneralClass.deposit = programPrice;
        //    return (programPrice);
        //}
        public async Task<string> GetPriceByProgramOptionId(int ProgramOptionId)
        {
            var programPrice = await _projectServices.GetPriceByProgramOptionId(Convert.ToInt32(ProgramOptionId));
            // var Price = programPrice.Split(',')[0];
            GeneralClass.deposit = programPrice;
            return (programPrice);
        }
        public async Task<string> programDepositByProgramId(string ProgramId)
        {
           // var programPrice = await _projectServices.GetPriceByProgramId(Convert.ToInt32(ProgramId));
            var Price = GeneralClass.deposit.Split(',')[1];
            return (Price);
        }
        public async Task<string> programDurationByProgramId(string ProgramId)
        {
           // var programPrice = await _projectServices.GetPriceByProgramId(Convert.ToInt32(ProgramId));
            var Price = GeneralClass.deposit.Split(',')[2];
            return (Price);
        }

        public async Task<IActionResult> DeleteProgram(int Id)
        {
            var sessionValue = HttpContext.Session.GetString(SessionUsername);
            if (sessionValue == null)
            {
                _notyfService.Error("Kindly Login", 5);
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            var user = await _userManager.FindByNameAsync(sessionValue);
            if (user == null)
            {
                _notyfService.Error("Invalid user", 5);
                return RedirectToAction("Login", "Home", new { area = "" });

            }
            var response = await _projectServices.DeleteUserProgramOption(Id);
            var dashboardRecord = new DashboardVM();
            return View();
            //dashboardRecord.deleteSource = GeneralClass.source;
            //if (dashboardRecord.deleteSource == "Dashboard")
            //{
            //    var record = await _projectServices.DashboardRe(GeneralClass.email);
            //    if (record != null)
            //    {
            //        dashboardRecord.fullName = record.fullName;
            //        dashboardRecord.programCategpryName = record.programCategpryName;
            //        dashboardRecord.PaymentRecord = record.PaymentRecord;
            //        dashboardRecord.SubjectRecord = record.SubjectRecord;
            //        dashboardRecord.ChoicesRecord = record.ChoicesRecord;
            //        dashboardRecord.programCategpryName = record.programCategpryName;
            //        dashboardRecord.programListz = await _projectServices.GetPrograms();
            //        dashboardRecord.rCode = record.rCode;
            //    }
            //    return View(dashboardRecord.deleteSource, dashboardRecord);
            //}
            //else
            //{
            //    var response2 = await _projectServices.UserPrograms( GeneralClass.email);
            //    var programs = await _projectServices.GetPrograms();
            //    var result = new DashboardVM();
            //    result.ProgramCategorys = response2;
            //    result.deleteSource = "AllPrograms";
            //    result.programListz = programs;
            //    return View(dashboardRecord.deleteSource, result);
            //}
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<string> getDiscountRate(string GetDiscountRate)
        {
            var disRate = await _projectServices.GetDiscountRate(GetDiscountRate);
            return (disRate);
        }
        public async Task<string> GetPromoCode(string Code)
        {
            var cityList = await _projectServices.GetPromoCode(Code);
            return (cityList);
        }
    }
}
