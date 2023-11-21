using CPEA.Utilities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class ReportVM
    {
        public List<ReportDetails> RPrograms { get; set; }
        public List<ReportDetails> RCategories { get; set; }
        public List<ReportDetails> RCourses { get; set; }
        public List<ReportDetails> RCertifications { get; set; }
        public ReportDTO dto { get; set; }
       
    }
    public class ReportStat
    {
        public string Total { get; set; }
        public string Failed { get; set; }
        public string Successful { get; set; }
        public string Pending { get; set; }
    }
    public class ReportDetails
    {
        public string Name { get; set; }
        public string TotalStudent { get; set; }
        public string TotalActive { get; set; }
        public string TotalInactive { get; set; }
        //public string TotalPartial { get; set; }
        //public string TotalCompleted { get; set; }
        public string TotalAmountPaid { get; set; }
    }
    public class details
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public UserStatusEnums Status { get; set; }
    }

}
