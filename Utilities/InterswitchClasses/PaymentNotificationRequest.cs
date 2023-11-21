using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class PaymentNotificationRequest
    {
        public string ServiceUrl { get; set; }
        public string ServiceUsername { get; set; }
        public string SeervicePassword { get; set; }
        public string FTPUrl { get; set; }
        public string FTPUsername { get; set; }
        public string FTPassword { get; set; }
        public string IsRepeated { get; set; }
        public string ProductGroupCode { get; set; }
        public string PaymentLogId { get; set; }
        public string CustReference { set; get; }
        public string AlternateCustReference { get; set; }
        public string  Amount { get; set; } //M
        public int PaymentStatus { get; set; } //M
        public string PaymentMethod { get; set; } //M
        public string PaymentReference { get; set; } //M
        public string TerminalId { get; set; }
        public string ChannelName { get; set; } //M
        public string Location { get; set; }
        public string IsReversal { get; set; } //M
        public string PaymentDate { get; set; } //M
        public string SettlementDate { get; set; } //M
        public string InstitutionId { get; set; } //M
        public string InstitutionName { get; set; } //M
        public string BranchName { get; set; }
        public string BankName { get; set; }
        public string FeeName { get; set; }
        public string CustomerName { get; set; } //M
        public string OtherCustomerInfo { get; set; }
        public string ReceiptNo { get; set; }
        public string CollectionsAccount { get; set; }
        public string ThirdPartyCode { get; set; }
        public string BankCode { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string DepositorName { get; set; }
        public string DepositorSlipNumber { get; set; }
        public int PaymentCurrency { get; set; } //M
        public string OriginalPaymentLogId { get; set; }
        public string OriginalPaymentReference { get; set; }
        public string Teller { get; set; }
        public string ItemName { get; set; } //M
        public string ItemCode { get; set; } //M
        public int ItemAmount { get; set; }
        public string LeadBankCode { get; set; }
        public string LeadBankCbnCode { get; set; }
        public string LeadBankName { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }

    public class Payments
    {
        public Payment Payment { set; get; }
    }

    public class Payment
    {

        public string IsRepeated { get; set; }

        public string ProductGroupCode { get; set; }

        public int PaymentLogId { get; set; }

        public string CustReference { set; get; }

        public string AlternateCustReference { get; set; }

        public double Amount { get; set; } //M

        public int PaymentStatus { get; set; } //M

        public string PaymentMethod { get; set; } //M

        public string PaymentReference { get; set; } //M

        public string TerminalId { get; set; }

        public string ChannelName { get; set; } //M

        public string Location { get; set; }

        public string IsReversal { get; set; } //M

        public string PaymentDate { get; set; } //M

        public string SettlementDate { get; set; } //M

        public string InstitutionId { get; set; } //M

        public string InstitutionName { get; set; } //M

        public string BranchName { get; set; }

        public string BankName { get; set; }

        public string FeeName { get; set; }

        public string CustomerName { get; set; } //M

        public string OtherCustomerInfo { get; set; }

        public string ReceiptNo { get; set; }

        public string CollectionsAccount { get; set; }

        public string ThirdPartyCode { get; set; }

        public string BankCode { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerPhoneNumber { get; set; }

        public string DepositorName { get; set; }

        public string DepositorSlipNumber { get; set; }

        public int PaymentCurrency { get; set; } //M

        public PaymentItem PaymentItems { get; set; }

        public string OriginalPaymentLogId { get; set; }

        public string OriginalPaymentReference { get; set; }

        public string Teller { get; set; }

    }

    public class PaymentItem
    {
        public string ItemName { get; set; } //M

        public string ItemCode { get; set; } //M

        public int ItemAmount { get; set; }

        public string LeadBankCode { get; set; }

        public string LeadBankCbnCode { get; set; }

        public string LeadBankName { get; set; }

        public string CategoryCode { get; set; }

        public string CategoryName { get; set; }
    }
}
