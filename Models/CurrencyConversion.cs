using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class CurrencyConversion
    {
        [Key]
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public float ConversionRate { get; set; }
    }
}
