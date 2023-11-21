using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class CoursePriceOptions
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public float Amount { get; set; }
        public int CourseId { get; set; }
        public Courses Course { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
      //  public float Charges { get; set; }

    }
}
