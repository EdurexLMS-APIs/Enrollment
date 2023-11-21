using CPEA.Utilities.Interface;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly TermiiSettings _termiiSettings;
        private readonly SendGridSettings _sendGridSettings;
        public MessagingService(IOptions<TermiiSettings> termiiSettings, IOptions<SendGridSettings> sendGridSettings)
        {
            _sendGridSettings = sendGridSettings.Value;
            _termiiSettings = termiiSettings.Value;
        }
        public async Task<bool> SendEmail(string email, string message, string subject)
        {
            try
            {
                return await SendGridProvider(email, message, subject);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> SendSMS(string phone, string message)
        {
            try
            {
                return await TermiiProvider(phone, message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> SendGridProvider(string email, string message, string subject)
        {
            try
            {
                string apiKey = _sendGridSettings.apikey;
                string senderEmail = _sendGridSettings.Email;
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(senderEmail, "Edurex Academy");
                var to = new EmailAddress(email, "Edurex Academy User");
                var htmlContent = message;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "Caroline my friend.. u can use this parameter to send plain text email by removing the last param and typing in your email text here", htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    var val = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        public async Task<bool> TermiiProvider(string phone, string message)
        {
            try
            {

                var mailRequest = new TermiiEmailRequest()
                {
                    to = phone,
                    from = _termiiSettings.from,
                    sms = message,// message,
                    type = "plain",
                    api_key = _termiiSettings.apikey,
                    channel = "generic",
                    media = null
                };
                var jsonstr = $"{JsonConvert.SerializeObject(mailRequest)}";
                var client = new RestClient("https://api.ng.termii.com/api/sms");
                //client.Timeout = -1;
                var request = new RestRequest("send", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", jsonstr, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                var val = response.StatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
