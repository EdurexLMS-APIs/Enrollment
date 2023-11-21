using CPEA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserWalletHistory
    {
        [Key]
        public int Id { get; set; }
        public int WalletId { get; set; }
        public UserWallet Wallet { get; set; }
        public WalletEnums TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
