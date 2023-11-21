using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class Receipts
    {
        [Key]
        public int Id { get; set; }
        public string ReceiptId { get; set; }
        public string PaymentRef { get; set; }
    }
}
