using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserSubjects
    {
        [Key]
        public int Id { get; set; }
        public string SubjectIds { get; set; }
        //public Subjects Subject { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
