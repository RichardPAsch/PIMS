using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Income 
    {
        // Considered as child object of Aggregate root (Asset).
        public virtual string Url { get; set; }

        [Required]
        public virtual Guid IncomeId { get; set; }

        public virtual string Account { get; set; }

        [Required]
        [Range(0.01, 5000.00)]
        public virtual decimal Actual { get; set; }

        // Projected amount based on asset profile.
        public virtual decimal Projected { get; set; }

        [Required]
        public virtual string DateRecvd { get; set; }

       
    }
}
