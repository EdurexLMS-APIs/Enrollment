using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserChoices
    {
        [Key]
        public int Id { get; set; }
        public int? ProgramId { get; set; }
        public Programs Program { get; set; }
        public int? InstitutionId { get; set; }
        public Institutions Institution { get; set; }
        public int? CourseId { get; set; }
        public Courses Course { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
