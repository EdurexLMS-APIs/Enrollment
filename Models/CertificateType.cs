using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class CertificateType
    {
        [Key]
        public int Id { get; set; }
        public string CertType { get; set; }
        public float Fee { get; set; }
    }
}
