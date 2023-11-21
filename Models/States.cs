using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class States
    {
        [Key]
        public int Id { get; set; }
        public int CountryId { get; set; }
        public Countries Country { get; set; }
        public string Name { get; set; }
        public float CourierFee { get; set; }
    }
}
