using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Income 
    {
        // Considered as child object of Aggregate root (Asset).
        public virtual string Url { get; set; }

        // NH PK match in Income table.
        [Required]
        public virtual Guid IncomeId { get; set; }

        // NH one-side reference for M:1 relationship to Asset.
        public virtual Asset IncomeAsset { get; set; }

       
        // Referenced by domain
        public virtual Guid AssetId { get; set; }

        // Account type.
        public virtual string Account { get; set; }

        [Required]
        [Range(0.01, 5000.00)]
        public virtual decimal Actual { get; set; }

        // Projected amount based on asset profile.
        public virtual decimal Projected { get; set; }

        [Required]
        [RegularExpression(@"^((((0[13578])|([13578])|(1[02]))[\/](([1-9])|([0-2][0-9])|(3[01])))|(((0[469])|([469])|(11))[\/](([1-9])|([0-2][0-9])|(30)))|((2|02)[\/](([1-9])|([0-2][0-9]))))[\/]\d{4}$|^\d{4}$")]
        public virtual string DateRecvd { get; set; }
        
       
        // Added 3-10-15 
        public virtual string LastUpdate { get; set; }


        // ** For future reference if needed **
        // NH requirement for composite keys: 'composite-id class must override Equals()'.
        //public override bool Equals(object obj) {
        //    if (obj == null || GetType() != obj.GetType())
        //        return false;

        //    var that = (Income)obj;

        //    return IncomeId == that.IncomeId && IncomeId == that.IncomeId;
        //}

        //public override int GetHashCode() {
        //    return (IncomeId.GetHashCode() ^ IncomeId.GetHashCode());
        //}

       
    }
}
