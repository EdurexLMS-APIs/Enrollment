using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    public class addressResponse
    {
        public string code { get; set; }
        public bool successful { get; set; }
        public string message { get; set; }
        public addressByCityId data { get; set; }
    }
    public class addressByCityId
    {
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
    }
    public class countryResponse
    {
        public List<countriesbyCountry> data { get; set; }
    }
    public class countriesbyCountry
    {
        public int countryId { get; set; }
        public string countryName { get; set; }
        public string iso { get; set; }
    }
    public class stateResponse
    {
        public StatesbyCountry data { get; set; }
    }
    public class StatesbyCountry
    {
        public int countryId { get; set; }
        public string countryName { get; set; }
        public List<states> states { get; set; }
    }
    public class states
    {
        public int stateId { get; set; }
        public string stateName { get; set; }
    }
    public class cityResponse
    {
        public CitiesByState data { get; set; }
    }
    public class CitiesByState
    {
        public int countryId { get; set; }
        public string countryName { get; set; }
        public int stateId { get; set; }
        public string stateName { get; set; }
        public List<cities> cities { get; set; }
    }
    public class cities
    {
        public int cityId { get; set; }
        public string cityName { get; set; }
    }
    public class streetResponse
    {
        public StreetByCity data { get; set; }
    }
    public class StreetByCity
    {
        public int countryId { get; set; }
        public string countryName { get; set; }
        public int stateId { get; set; }
        public string stateName { get; set; }
        public int cityId { get; set; }
        public string cityName { get; set; }
        public List<Streets> streetName { get; set; }
    }
    public class Streets
    {
        public int streetId { get; set; }
        public string streetName { get; set; }
        public int areaId { get; set; }
        public string areaName { get; set; }
        public int estateId { get; set; }
        public string estateName { get; set; }
    }
    public class locationResponse
    {
        public string code { get; set; }
        public bool successful { get; set; }
        public string message { get; set; }
        public location data { get; set; }
    }
    public class location
    {
        public int superId { get; set; }

        public int countryId { get; set; }
        public string countryName { get; set; }

        public int stateId { get; set; }
        public string stateName { get; set; }

        public int cityId { get; set; }
        public string cityName { get; set; }

        public int? areaId { get; set; }
        public string areaName { get; set; }

        public int? estateId { get; set; }
        public string estateName { get; set; }

        public int? streetId { get; set; }
        public string streetName { get; set; }

        public string streetNumber { get; set; }

        public string status { get; set; }

    }
}
