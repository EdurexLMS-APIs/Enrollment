using CPEA.Data;
using CPEA.Utilities.Interface;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CPEA.Utilities.Services
{
    public class EmailService : IEmail
    {
        private readonly ApplicationDbContext _context;
        private readonly SMTPSetting _smtpSettings;
        private readonly SendGridSettings _sendgridSettings;
        public EmailService(IOptions<SendGridSettings> sendgridSettings, ApplicationDbContext context, IOptions<SMTPSetting> smtpSettings)
        {
            _context = context;
            _smtpSettings = smtpSettings.Value;
            _sendgridSettings = sendgridSettings.Value;
        }
        public async Task<bool> SendEmail(string email, string message, string subject)
        {
            try
            {
                var result = await SendGridProvider(email, message, subject);
                return result;
                //var emailsentStatus = false;
                //using (var client = new SmtpClient())
                //{
                //    var credential = new NetworkCredential
                //    {
                //        UserName = _smtpSettings.Sender,// _configuration["Email:Email"],
                //        Password = _smtpSettings.Password//_configuration["Email:Password"]
                //    };

                //    bool useDefaultCredential = true;
                //    string checkCredential = "false";// _configuration["Email:UseDefaultCredential"];
                //    if (!string.IsNullOrEmpty(checkCredential))
                //    {
                //        useDefaultCredential = Convert.ToBoolean(checkCredential);
                //    }

                //    if (useDefaultCredential)
                //    {
                //        client.UseDefaultCredentials = true;
                //    }
                //    else
                //    {
                //        client.Credentials = credential;
                //    }
                //    client.Host = _smtpSettings.Server;//_configuration["Email:Host"];
                //    client.Port = _smtpSettings.Port;// 587;// int.Parse(587);
                //    client.EnableSsl = true;// Convert.ToBoolean(_configuration["Email:EnabledSSL"]);
                //    client.Timeout = 10000;


                //    using (var emailMessage = new MailMessage())
                //    {
                //        emailMessage.IsBodyHtml = false;
                //        emailMessage.To.Add(new MailAddress(email));
                //        emailMessage.From = new MailAddress(_smtpSettings.Sender, "Edurex ACADEMY");//_configuration["Email:Email"], "Respay");
                //        emailMessage.Subject = subject;
                //        emailMessage.Body = message;
                //        client.Send(emailMessage);
                //    }

                //    await Task.CompletedTask;

                //    emailsentStatus = true;

                //}
                //if (emailsentStatus == true)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                //string sender = "learn@edurex.academy";
                //string password = "bQ%YHuBRmdmB5uR>";
                //subject = "Greetings";
                //message = "How are you doing?";

                //var client = new SmtpClient("smtp.gmail.com", 587)
                //{
                //    EnableSsl = true,
                //    UseDefaultCredentials = false,
                //    Credentials = new NetworkCredential(sender, password)

                //};

                //var result = client.SendMailAsync(
                //    new MailMessage(
                //        from: sender,
                //        to: "ca@newagesolutions.com",
                //        subject,
                //        message
                //        ));

                //await result;

                //if (emailsentStatus == true)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //using (var client = new SmtpClient())
            //{
            //    var credential = new NetworkCredential
            //    {
            //        UserName = _smtpSettings.Sender,// _configuration["Email:Email"],
            //        Password = _smtpSettings.Password//_configuration["Email:Password"]
            //    };

            //    bool useDefaultCredential = true;
            //    string checkCredential = "false";// _configuration["Email:UseDefaultCredential"];
            //    if (!string.IsNullOrEmpty(checkCredential))
            //    {
            //        useDefaultCredential = Convert.ToBoolean(checkCredential);
            //    }

            //    if (useDefaultCredential)
            //    {
            //        client.UseDefaultCredentials = true;
            //    }
            //    else
            //    {
            //        client.Credentials = credential;
            //    }
            //    client.Host = _smtpSettings.Server;//_configuration["Email:Host"];
            //    client.Port = 465;// int.Parse(587);
            //    client.EnableSsl = true;// Convert.ToBoolean(_configuration["Email:EnabledSSL"]);
            //    client.Timeout = 10000;


            //    using (var emailMessage = new MailMessage())
            //    {
            //        emailMessage.IsBodyHtml = true;
            //        emailMessage.To.Add(new MailAddress(email));
            //        emailMessage.From = new MailAddress(_smtpSettings.Sender, "EDUREX");//_configuration["Email:Email"], "Respay");
            //        emailMessage.Subject = subject;
            //        emailMessage.Body = message;
            //        client.Send(emailMessage);
            //    }

            //    await Task.CompletedTask;

            //    emailsentStatus = true;

            //}
            //if (emailsentStatus == true)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            //using (MailMessage mm = new MailMessage(_smtpSettings.Sender, "ca@newagesolutions.com"))
            //{
            //    //    string sender = "learn@edurex.academy";
            //    //    string password = "bQ%YHuBRmdmB5uR>";
            //    //    subject = "Greetings";
            //    //    message = "How are you doing?";
            //    mm.Subject = "Greetings";
            //    string body = "How are you doing?";
            //    mm.Body = body;
            //    mm.IsBodyHtml = false;
            //    SmtpClient smtp = new SmtpClient();
            //    smtp.Host = _smtpSettings.Server;
            //    smtp.EnableSsl = true;
            //    NetworkCredential NetworkCred = new NetworkCredential(_smtpSettings.Sender, _smtpSettings.Password);
            //    smtp.UseDefaultCredentials = false;
            //    smtp.Credentials = NetworkCred;
            //    smtp.Port = 587;
            //    smtp.Send(mm);

            //    return true;
            //}

            //return false;
        }

        public async Task<bool> SendGridProvider(string email, string message, string subject)
        {
            try
            {
                var apiKey = _sendgridSettings.apikey;
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_sendgridSettings.Email, "Edurex ACADEMY");
                //var subject = subject;
                var to = new EmailAddress(email, "New User");
                var plainTextContent = "Edurex ACADEMY Email";
                var htmlContent = message;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);                

                if (response.IsSuccessStatusCode)
                {
                    var val = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message, ex);
                throw ex;
            }

            return true;
        }
    }
}
