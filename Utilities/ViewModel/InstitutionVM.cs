using CPEA.Models;
using CPEA.Utilities.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class InstitutionVM
    {
        public List<InstitutionRecord> InstitutionList { get; set; }
        public AddInstitutionDTO Institution { get; set; }
        public InstitutionEditVM institutionEdit { get; set; }
        public IEnumerable<Countries> countryListz { get; set; }
        public IEnumerable<States> nigeriaStatesListz { get; set; }
        public IEnumerable<InstitutionType> institutionTypesListz { get; set; }
        //public CoursesVM CoursesVM { get; set; }

    }
    public class InstitutionRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string InstitutionType { get; set; }
        public int CityId { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public UserStatusEnums Status { get; set; }
        public int TotalOfficer { get; set; }

    }

    public class AddInstitutionDTO
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int InstitutionTypeId { get; set; }
        public IFormFile Logo { get; set; }
        public int CityId { get; set; }
    }

}
