using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class CourseRegConfirmDTO
    {
        public string studentName { get; set; }
        public string studentId { get; set; }
        public string courseName { get; set; }
        public string courseCode { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string totalAmountPaid { get; set; }
        public List<CoursePayment> coursePayment { get; set; }
        public string orientationDateTime { get; set; }
        public string LMSUrl { get; set; }
        public string LMSUsername { get; set; }
        public string LMSPassword { get; set; }
        public string supportEmail { get; set; }
        public string supportPhone { get; set; }
    }

    public class CoursePayment
    {
        public PaymentMethodEnums paymentMethod { get; set; }
        public string paymentDate { get; set; }
        public string amountPaid { get; set; }
    }
}
