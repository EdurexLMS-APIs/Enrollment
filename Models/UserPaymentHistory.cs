using CPEA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserPaymentHistory
    {
        [Key]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatusEnums StatusId { get; set; }
        public string Description { get; set; }
        public string PaymentRef { get; set; }
        public bool CourseOptionDateChanged { get; set; } = false;
        public int ChangedToUserPaymentForId { get; set; } = 0;
        public PaymentMethodEnums PaymentMethodId { get; set; }
        public DateTime PaymentDate { get; set; }
        public paymentForEnums PaymentFor { get; set; }
        public int UserPaymentForId { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
