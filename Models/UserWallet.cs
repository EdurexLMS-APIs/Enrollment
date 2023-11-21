using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class UserWallet
    {
        [Key]
        public int Id { get; set; }
        public string WalletId { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal SavingBalance { get; set; }
        public decimal EscrowBalance { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
