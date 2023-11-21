using CPEA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class Certifications
    {
        [Key]
        public int Id { get; set; }
        public CertificationTypeEnums Mode { get; set; }
        public string OrganisationName { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        //public int AmountDollar { get; set; }
        public int CategoryId { get; set; }
        
        public ProgramCategory Category { get; set; }
    }
}
