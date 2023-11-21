using CPEA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class EditCourseOptionDTO
    {
        public int OptionId { get; set; }
        public CoursePriceOptions coursePriceOptions { get; set; }
    }
}
