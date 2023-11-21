using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class ProgramOptions
    {
        [Key]
        public int Id { get; set; }
        public string ProgramOptionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float PriceNGN { get; set; }
        public float DepositNGN { get; set; }
        public float PriceUSD { get; set; }
        public float DepositUSD { get; set; }
        public int CategoryId { get; set; }
        public ProgramCategory Category { get; set; }
        public string Duration { get; set; }
        public DateTime StartDate { get; set; }
        public int? MaxSubjectSelection { get; set; }
    }
}
