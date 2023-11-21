using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class PaymentDTO
    {
        public decimal Amount { get; set; }
        public PaymentMethodEnums PaymentMethod { get; set; }
        public string BankConnectPaymenttype { get; set; }
        public int userProgramOptionId { get; set; }
        public string ReferralDiscountCode { get; set; }

    }
    public class PaymentDTO2
    {
        public decimal Amount { get; set; }
       // public PaymentMethodEnums PaymentMethodId { get; set; }
        public paymentForEnums PaymentFor { get; set; }
        public string PaymentMethod { get; set; }
        public int UserPaymentForId { get; set; }
        public int UserOldPaymentForId { get; set; }
        public string UserId { get; set; }
        public string paymentRef { get; set; }
        public string Description { get; set; }
       // public float perAmount { get; set; } 
        public float discountAm { get; set; } 
    }
}
