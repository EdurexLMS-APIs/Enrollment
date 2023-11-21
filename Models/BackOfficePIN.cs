using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class BackOfficePIN
    {
        [Key]
        public int Id { get; set; }
        public string PIN { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
