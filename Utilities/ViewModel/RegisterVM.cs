using CPEA.Models;
using CPEA.Utilities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class RegisterVM
    {
        public string refCode { get; set; }
        public RegisterDTO registerz { get; set; }
        public IEnumerable<Programs> programListz { get; set; }
        // public IEnumerable<ProgramCategory> programCategoryListz { get; set; }
        public IEnumerable<CountryDetails> countryListz { get; set; }
        public IEnumerable<StateDetails> nigeriaStatesListz { get; set; }
    }
    public class Register1VM
    {
        public RegisterUserDTO registerz { get; set; }
        public IEnumerable<Countries> countryListz { get; set; }
        public IEnumerable<States> nigeriaStatesListz { get; set; }
        public string refCode { get; set; }
        public string percentageOffer { get; set; }
    }
    public class RegisterNewVM
    {
        public IEnumerable<Institutions> programListz { get; set; }
        public List<GetDataModem> DataList { get; set; }
        public List<GetDataModem> ModemList { get; set; }
        public string phoneNumber { get; set; }
        public string refCode { get; set; }
        public Register2DTO register2DTO { get; set; }
        public bool CertificationOnly { get; set; } = false;
        public int percentageOffer { get; set; } = 0;
    }
    public class GetDataModem
    {
        public int Id { get; set; }
        public string NetworkProvider { get; set; }
        public string Duration { get; set; }
        public string Bundle { get; set; }
        public string Amount { get; set; }
    }
    public class CourseOptionDates
    {
        //public int Id { get; set; }
        public string DateRange { get; set; }
        public string institutionName { get; set; }
        
    }
    public class CourseOptionDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CertificateOptionDates
    {
        public int Id { get; set; }
        public string ExamDate { get; set; }
    }
}
