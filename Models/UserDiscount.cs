using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserDiscount
    {
        [Key]
        public int Id { get; set; }
        public string ReferralId { get; set; }
        public Users Referral { get; set; }
        public float Rate { get; set; }
        public DateTime RegDate { get; set; }
        public int TotalApproved { get; set; }
        public int TotalApplied { get; set; }
        public string Code { get; set; }
    }
}
