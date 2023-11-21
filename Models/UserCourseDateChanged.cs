using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserCourseDateChanged
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public int? ChangedFromId { get; set; }
        public UserCourses ChangedFrom { get; set; }
        public int? ChangedToId { get; set; }
        public UserCourses ChangedTo { get; set; }
        public DateTime ChangedDate { get; set; }
    }
}
