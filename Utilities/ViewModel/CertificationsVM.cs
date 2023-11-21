using CPEA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class CertificationsVM
    {
        public List<Certi> Certifications { get; set; }
        //public IEnumerable<Programs> programListz { get; set; }
        public IEnumerable<ProgramCategory> programCatListz { get; set; }
        public IEnumerable<Currency> currencyListz { get; set; }
        public Certifications Certification { get; set; }
        public CertificationPriceOptions NewPriceOption { get; set; }
        public CertPriceOptionVM CertPriceOptionVM { get; set; }
    }
    public class Certi
    {
        public int Id { get; set; }
        public CertificationTypeEnums Mode { get; set; }
        public string OrganisationName { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        //public string Amount { get; set; }
        public string Category { get; set; }
        public string Program { get; set; }
    }

    public class CertPriceOptionVM
    {
        public List<CertPriceOptionV> CertPriceOptionV { get; set; }
        public string certName { get; set; }
        public int certId { get; set; }
    }
    public class CertPriceOptionV
    {
        public int Id { get; set; }
        public string ExamDate { get; set; }
        public string Amount { get; set; }
        public string Charges { get; set; }
        public string Currency { get; set; }
        public string certName { get; set; }

    }
}
