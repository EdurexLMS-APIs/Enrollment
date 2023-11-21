using CPEA.Data;
using CPEA.Utilities;
using CPEA.Utilities.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CPEA
{
    public class AddressServices : IAddressServices
    {
        private static readonly HttpClient addressClient;

        static AddressServices()
        {           
            addressClient = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:44363/")//http://lcs.respay.com/api/")
            };
        }
        public async Task<countryResponse> GetAllCountry()
        {
            var countries = new countryResponse();
            HttpResponseMessage response = addressClient.GetAsync($"v1/countries").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<countryResponse>(jsonString);
                return countries;
            }
            return null;
        }
        public async Task<stateResponse> GetAllStateByCountryId(int CountryId)
        {
            var countries = new stateResponse();
            HttpResponseMessage response = addressClient.GetAsync($"v1/countries/{CountryId}/state").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<stateResponse>(jsonString);
                return countries;
            }
            return null;
        }

        public async Task<cityResponse> GetAllCityByStateId(int stateId)
        {
            var countries = new cityResponse();
            HttpResponseMessage response = addressClient.GetAsync($"v1/states/{stateId}/cities").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<cityResponse>(jsonString);
                return countries;
            }
            return null;
        }
        public async Task<streetResponse> GetAllStreetByCityId(int cityId)
        {
            var countries = new streetResponse();
            HttpResponseMessage response = addressClient.GetAsync($"v1/cities/{cityId}/streets").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<streetResponse>(jsonString);
                return countries;
            }
            return null;
        }

        public async Task<locationResponse> CreateLocation(int cityId, string streetName, string streetNumber)
        {
            var countries = new locationResponse();

            var dto = new LocationCreateDTO()
            {
                cityId = cityId,
                streetName = streetName,
                streetNumber = streetNumber,
            };
            
            HttpResponseMessage response =await addressClient.PostAsync("v1/locations", new StringContent(JsonConvert.SerializeObject(dto),Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<locationResponse>(jsonString);

                return countries;
            }
            else
            {
                return null;
            }
        }
        public async Task<locationResponse> GetLocation(int superId)
        {
            var countries = new locationResponse();
            HttpResponseMessage response = addressClient.GetAsync($"v1/locations/{superId}").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                countries = JsonConvert.DeserializeObject<locationResponse>(jsonString);

                return countries;
            }
            return null;
        }
        public async Task<string> GetAddressByCityId(int cityId)
        {
            if(cityId > 0)
            {
                var countries = new addressResponse();
                HttpResponseMessage response = addressClient.GetAsync($"v1/locations/AddressByCityId/{cityId}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    countries = JsonConvert.DeserializeObject<addressResponse>(jsonString);

                    return countries.data.city+"," +countries.data.state+","+countries.data.country;
                }
            }
           
            return null;
        }
    }
}
