using CPEA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserCourses
    {
        [Key]
        public int Id { get; set; }
        public int CoursePriceOptionId { get; set; }
        public CoursePriceOptions CoursePriceOption { get; set; }
        public int CertificateTypeId { get; set; }
       // public CertificateType CertificateType { get; set; }
        public int deliveryStateId { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public DateTime RegisteredDate { get; set; }
        public UserCourseStatusEnums Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool CertificateIssued { get; set; } = false;
        public DateTime? CertificateIssuedDate { get; set; }
        public UserProgramPaymentStatusEnums PaymentStatus { get; set; }
    }
}
