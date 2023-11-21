using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    //public enum CurrencyEnums
    //{
    //    [Description("₦")]
    //    Naira = 1,
    //    [Description("$")]
    //    Dollar = 2
    //}
    public enum PromoCategoryEnums
    {
        [Description("NYSC")]
        NYSC = 1
    }
    public enum WalletEnums
    {
        [Description("Credit")]
        Credit = 1,
        [Description("Dedit")]
        Dedit = 2
    }
    public enum CertificationTypeEnums
    {
        [Description("Local")]
        Local = 1,
        [Description("Foreign")]
        Foreign = 2
    }
    public enum UserCourseStatusEnums
    {
        [Description("Pending")]
        Pending = 1,
        [Description("In Progress")]
        InProgress = 2,
        [Description("Completed")] 
        Completed = 3,
        [Description("Canceled")]
        Canceled = 4
    }
    public enum StaffDepEnums
    {
        [Description("None")]
        None = 0,
        [Description("Admin")]
        Admin = 1,
        [Description("Customer Support")]
        Customer_Support = 2,
        [Description("Marketing")]
        Marketing = 3
    }
    public enum UserRolesEnums
    {
        [Description("Student")]
        Student = 1,
        [Description("Admin")]
        Admin = 2,
        [Description("Customer Support")]
        Customer_Support = 3,
        [Description("Marketing")]
        Marketing = 4,
        [Description("Supervisor")]
        Supervisor = 5,
        [Description("Staff")]
        Staff = 6,
        [Description("Freelance")]
        Freelance = 7

    }
    public enum UserStatusEnums
    {
        [Description("Active")]
        Active = 1,
        [Description("Inactive")]
        Inactive = 2,
        [Description("Blocked")]
        Blocked = 3
    }
    public enum UserProgramPaymentStatusEnums
    {
        [Description("Outstanding")]
        Outstanding = 1,
        [Description("Deposited")]
        Deposited = 2,
        [Description("Paid")]
        Paid = 3
    }
    public enum PaymentMethodEnums
    {
        [Description("Card")]
        Card = 1,
        [Description("Account Transfer")]
        AccountTransfer = 2,
        [Description("Bank COnnect")]
        BankConnect = 3,
        [Description("Discount")]
        Discount = 4,
        [Description("Offline")]
        Offline = 5,
        [Description("InterSwitch")]
        InterSwitch = 6,
        [Description("Promo")]
        Promo = 7,
        [Description("Referral")]
        Referral = 8,
    }
    public enum paymentForEnums
    {
        [Description("Course")]
        Course = 1,
        [Description("Certifications")]
        Certifications = 2,
        [Description("Data")]
        Data = 3,
        [Description("Modem")]
        Modem = 4,
        [Description("PhysicalCertificate")]
        PhysicalCertificate = 5,
        [Description("Courier")]
        Courier =6 ,
        [Description("Promo")]
        Promo = 7
    }
    public enum PaymentStatusEnums
    {
        [Description("Initialized")]
        Initialized = 1,
        [Description("In Progress")]
        Pending = 2,
        [Description("Failed")]
        Failed = 3,
        [Description("Paid")]
        Paid = 4,
    }
    public enum UserProgramStatusEnums
    {
        [Description("Pending")]
        Pending = 1,
        [Description("In Progress")]
        InProgress = 2,
        [Description("Completed")]
        Completed = 3
    }

    public static class EnumHelper
    {
        public static string ToDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

    }
}
