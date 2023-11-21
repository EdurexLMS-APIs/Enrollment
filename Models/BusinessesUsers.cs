using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class BusinessesUsers
    {
        [Key]
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public Businesses Business { get; set; }
        public string UserRole { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
