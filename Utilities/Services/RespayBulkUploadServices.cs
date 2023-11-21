using CPEA.Data;
using CPEA.Utilities.Interface;
using CPEA.Utilities.RespayClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CPEA.Utilities.Services
{
    public class RespayBulkUploadServices : IRespayBulkUpload
    {
        private static readonly HttpClient client;

        static RespayBulkUploadServices()
        {
            client = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:44382")
            };
        }
        public async Task<JSonResponse> signin()
        {
            //var url = string.Format("api/v1/Auth/signin");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            LoginDTO emp = new LoginDTO() { username = "camukunra@newage.ng", password = "123456" };
            HttpResponseMessage response = await client.PostAsync("/api/v1/Auth/signin", new StringContent(JsonConvert.SerializeObject(emp), Encoding.UTF8, "application/json"));
            //return response;
            var result = new JSonResponse();
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<JSonResponse>(jsonString);
                return result;
            }
            //else
            //{
            //    throw new HttpRequestException(response.ReasonPhrase);

            //    return response.ReasonPhrase;
            //}
            return result;

        }

        public async Task<string> UploadPO(BulkPOUpload dto)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GeneralClass.authToken);
            HttpResponseMessage response = await client.PostAsync("AddBulkPropertyOwner", new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return "Successful";
            }
            else
            {
                return "Error";
            }
        }
    }
}
