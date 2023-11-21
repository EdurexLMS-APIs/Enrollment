using CPEA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class Institutions
    {
        [Key]
        public int Id { get; set; }
        public string LogoPath { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int InstitutionTypeId { get; set; }
        public InstitutionType InstitutionType { get; set; }
        public int CityId { get; set; }
        public UserStatusEnums Status { get; set; }
    }
}
