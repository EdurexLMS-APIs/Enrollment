using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IAddressServices
    {
        Task<countryResponse> GetAllCountry();
        Task<stateResponse> GetAllStateByCountryId(int CountryId);
        Task<cityResponse> GetAllCityByStateId(int stateId);
        Task<streetResponse> GetAllStreetByCityId(int cityId);
        Task<locationResponse> GetLocation(int superId);
        Task<locationResponse> CreateLocation(int cityId, string streetName, string streetNumber);
        Task<string> GetAddressByCityId(int cityId);

    }
}
