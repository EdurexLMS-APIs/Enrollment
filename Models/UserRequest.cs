using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserRequest
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public string Title { get; set; }
        public string Request { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? RespondedDate { get; set; }
        public bool Responded { get; set; } = false;
    }
}
