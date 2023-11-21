using CPEA.Models;
using CPEA.Utilities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class ProgramCategoryVM
    {
        public List<ProgramCategory> Category { get; set; }
       // public IEnumerable<Programs> programListz { get; set; }
        public IEnumerable<InstitutionRecord> institutionListz { get; set; }
        public ProgramCategory NewCategory { get; set; }
    }
    public class CoursesVM
    {
        public List<Cour> Courses { get; set; }
       // public IEnumerable<Programs> programListz { get; set; }
        public IEnumerable<Institutions> institutionListz { get; set; }
        public Courses Course { get; set; }
        public CoursePriceOptions NewPriceOption { get; set; }
        public CoursePriceOptionVM CoursePriceOptionVM { get; set; }
        public EditCourseDTO editCourseDTO { get; set; }
        public EditCourseOptionDTO editCourseOptionDTO { get; set; }
    }
    public class Cour
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Program { get; set; }
        public string InstitutionCode { get; set; }
        public int InstitutionId { get; set; }
        public int CategoryId { get; set; }


       // public string StartDate { get; set; }
        //public string Amount { get; set; }


    }
}
