using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class AllPayments
    {
        public int Id { get; set; }
        public string Amount { get; set; }
        public string StudentId { get; set; }
        public PaymentStatusEnums StatusId { get; set; }
        public string PaymentReference { get; set; }
        public string TransactionReference { get; set; }
        public PaymentMethodEnums PaymentMethodId { get; set; }
        public string PaymentDate { get; set; }
        public string Program { get; set; }
        public string Payee { get; set; }
    }
    public class PaymentVM
    {
        public List<AllPayments> AllPayments { get; set; }
        public ReportStat TodayRe { get; set; }
        public ReportStat WeekRe { get; set; }
        public ReportStat MonthRe { get; set; }
        public ConfirmPaymentVM ConfirmPayment { get; set; }
    }
    public class ConfirmPaymentVM
    {
       public string PIN { get; set; }
       public string Id { get; set; }
       public string AdminId { get; set; }
    }
}
