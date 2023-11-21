using CPEA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class InstitutionEditVM
    {
        public int InstitutionId { get; set; }
        public InstitutionEdit institutionEdit { get; set; }
    }
    public class InstitutionEdit
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int InstitutionTypeId { get; set; }
        public int CityId { get; set; }
    }

    public class InstitutionDestOffVM
    {
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public List<DeskOfficers> institutionDestO { get; set; }
        public InstitutionDestOffAdd addDeskOfficer { get; set; }
        public InstitutionDeskOfficerEdit editDeskOfficer { get; set; }
    }
    public class InstitutionDestOffAdd
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool CanLogin { get; set; }
        public int InstitutionId { get; set; }
    }

    public class InstitutionDeskOfficerEdit
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public bool CanLogin { get; set; }
    }
}
