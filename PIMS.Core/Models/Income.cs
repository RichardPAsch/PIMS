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

        public virtual Guid AssetId { get; set; }

        public virtual string Account { get; set; }

        [Required]
        [Range(0.01, 5000.00)]
        public virtual decimal Actual { get; set; }

        // Projected amount based on asset profile.
        public virtual decimal Projected { get; set; }

        [Required]
        [RegularExpression(@"^((((0[13578])|([13578])|(1[02]))[\/](([1-9])|([0-2][0-9])|(3[01])))|(((0[469])|([469])|(11))[\/](([1-9])|([0-2][0-9])|(30)))|((2|02)[\/](([1-9])|([0-2][0-9]))))[\/]\d{4}$|^\d{4}$")]
        public virtual string DateRecvd { get; set; }

       
    }
}
