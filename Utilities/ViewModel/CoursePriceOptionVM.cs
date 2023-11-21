using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class CoursePriceOptionVM
    {
        public string source { get; set; }
        public List<CoursePriceOptionV> CoursePriceOptionV { get; set; }
        public string courseName { get; set; }
        public int courseId { get; set; }

    }
    public class CoursePriceOptionV
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Course { get; set; }
        public string Duration { get; set; }
        public string Amount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
      //  public string Charges { get; set; }

    }
}
