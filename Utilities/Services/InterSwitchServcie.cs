using CPEA.Data;
using CPEA.Utilities.Interface;
using CPEA.Utilities.InterswitchClasses;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CPEA.Utilities.Services
{
    public class InterSwitchServcie : IInterSwitch
    {
        private readonly ApplicationDbContext _context;
        public InterSwitchServcie( ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> PaymentValidation(string request)
        {
            //Posting text/xml data to API
            //----------------------------
            string Uri = "https://my.edurex.academy/api/Interswitch/Payment/Validation";
            String result = "";
            using (HttpClient httpClient = new HttpClient())
            {
                var payload = new StringContent(request, Encoding.UTF8, "text/xml");
                try
                {
                    var response = httpClient.PostAsync(Uri, payload).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                        
                        if (result.Contains("CustomerInformationResponse"))
                        {
                            var customerRes = Deserializer<CustomerInformationResponse>(result);

                            if(customerRes.Status == 0)
                            {
                                result = "Valid Student";
                            }
                            else
                            {
                                result = "Invalid Student";
                            }

                        }
                        else if(result.Contains("PaymentNotificationResponse"))
                        {
                            var PaymentRes = Deserializer<PaymentNotificationResponse>(result);
                            if (PaymentRes.PaymentLogId != null)
                            {
                                var getPayment = await _context.InterswitchPaymentHistory.Where(x => x.PaymentLogId == PaymentRes.PaymentLogId).FirstOrDefaultAsync();
                                if(PaymentRes.Status == 0)
                                {
                                    getPayment.Status = PaymentStatusEnums.Paid;

                                    result = "Successful";
                                }
                                else
                                {
                                    getPayment.Status = PaymentStatusEnums.Failed;
                                    result = "Failed";
                                }

                                _context.InterswitchPaymentHistory.Update(getPayment);
                                await _context.SaveChangesAsync();
                            }
                        }
                        return result;
                    }
                }
                catch (Exception ex)
                {
                   var error = ex.Message;
                    return error;
                }
                return result;
            }

            ////Posting application/json data to API
            ////----------------------------
            ///var valr =
            //var objectToSend = Deserializer<CustomerInformationRequest>(request);

            //var result = new CustomerInformationResponse();
            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            //     HttpResponseMessage response = client.PostAsJsonAsync("https://localhost:44395/WeatherForecast/PaymentI", objectToSend).Result;

            //    if (response.IsSuccessStatusCode)
            //    {
            //        result = await response.Content.ReadAsAsync<CustomerInformationResponse>();

            //        return result;
            //    }
            //    return null;
            //}
        }
                
        private T Deserializer<T>(string data)
        {
            T instance;
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                using (var stringreader = new StringReader(data))
                {
                    instance = (T)xmlSerializer.Deserialize(stringreader);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return instance;
        }

        private string Serialize<T>(object data)
        {
            XmlSerializer xmlDeserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlDeserializer.Serialize(writer, data); //, xname, "UTF8"
                return stringWriter.ToString();
            }
        }

    }
}
