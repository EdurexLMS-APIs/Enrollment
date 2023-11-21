using CPEA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class EditCourseDTO
    {
        public int CourseId { get; set; }
        public Courses CourseDetails { get; set; }
    }
}
