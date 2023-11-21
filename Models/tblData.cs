using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class tblData
    {
        [Key]
        public int Id { get; set; }
        public string NetworkProvider { get; set; }
        public string Duration { get; set; }
        public string Bundle { get; set; }
        public string Amount { get; set; }
    }
}
