using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class Countries
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Iso { get; set; }
        public string Iso3 { get; set; }
    }
}
