using CPEA.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPEA.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> users { get; set; }
        //public DbSet<ProgramOptions> ProgramOptions { get; set; }
        public DbSet<Programs> Programs { get; set; }
        //public DbSet<ProgramCategory> ProgramCategory { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<States> States { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<Banks> Banks { get; set; }
        public DbSet<AffiliateUserProgram> AffiliateUserProgram { get; set; }
        public DbSet<AffiliateUserAccount> AffiliateUserAccount { get; set; }
        public DbSet<MarketerCode> MarketerCode { get; set; }
        public DbSet<Marketers> Marketers { get; set; }
        public DbSet<ProgramCategory> ProgramCategory { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Institutions> Institutions { get; set; }
        public DbSet<ProgramOptions> ProgramOptions { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<UserChoices> UserChoices { get; set; }
        public DbSet<UserPaymentHistory> UserPaymentHistory { get; set; }
        public DbSet<UserSubjects> UserSubjects { get; set; }
        public DbSet<UserProgramOption> UserProgramOption { get; set; }
        public DbSet<UserReferred> UserReferred { get; set; }
        public DbSet<UserDiscount> UserDiscount { get; set; }
        public DbSet<DiscountUsageHistory> DiscountUsageHistory { get; set; }
        public DbSet<LoginTrail> LoginTrail { get; set; }
        public DbSet<UserCourses> UserCourses { get; set; }
        public DbSet<UserCertifications> UserCertifications { get; set; }
        public DbSet<BackOfficePIN> BackOfficePIN { get; set; }
        public DbSet<Certifications> Certifications { get; set; }
        public DbSet<AllUserRoles> AllUserRoles { get; set; }
        public DbSet<CoursePriceOptions> CoursePriceOptions { get; set; }
        public DbSet<CertificationPriceOptions> CertificationPriceOptions { get; set; }
       // public DbSet<UserPaymentForLinker> UserPaymentForLinker { get; set; }
        public DbSet<InstitutionType> InstitutionType { get; set; }
        public DbSet<DeskOfficers> DeskOfficers { get; set; }
        public DbSet<tblData> tblData { get; set; }
        public DbSet<tblModem> tblModem { get; set; }
        public DbSet<CertificateType> CertificateType { get; set; }
        public DbSet<CurrencyConversion> CurrencyConversion { get; set; }
        public DbSet<Businesses> Businesses { get; set; }
        public DbSet<BusinessesUsers> BusinessesUsers { get; set; }
        public DbSet<UserDevices> UserDevices { get; set; }
        public DbSet<UserData> UserData { get; set; }        
        public DbSet<Receipts> Receipts { get; set; }        
        public DbSet<UserRequest> UserRequest { get; set; }        
        public DbSet<UserWallet> UserWallet { get; set; }        
        public DbSet<UserWalletHistory> UserWalletHistory { get; set; }        
        public DbSet<UserCourseDateChanged> UserCourseDateChanged { get; set; }        
        public DbSet<InterswitchPaymentHistory> InterswitchPaymentHistory { get; set; }        
        public DbSet<Promo> Promo { get; set; }        
        public DbSet<PromoUsageHistory> PromoUsageHistory { get; set; }        
        public DbSet<UserReferralPaymentHistory> UserReferralPaymentHistory { get; set; }        
        public DbSet<UserRoles> UserRole { get; set; }        
        public DbSet<Role> Role { get; set; }        
    }
}
