using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Position
    {
        // Considered as child object of Aggregate root (Asset).
        public virtual string Url { get; set; }

        // NH relational mapping : 'one' side of Asset
        public virtual Asset PositionAsset { get; set; }

        // NH - composite FK ref.
        public virtual Guid PositionAssetId { get; set; }

        // NH - composite FK ref.
        //public virtual Guid PositionInvestorId { get; set; }

        // NH - for PK mapping
        [Required]
        public virtual Guid PositionId { get; set; }

        // NH - FK 'one' side of Position/AccountType rel.
        public virtual AccountType Account { get; set; }

        // NH - FK ref. 
        public virtual Guid AcctTypeId { get; set; }



        public virtual string InvestorKey { get; set; }
        
        [Required]
        public virtual string PurchaseDate { get; set; }

        [Required]
        [Range(1,10000)]
        public virtual int Quantity { get; set; }
        
        

        // Useful for most recent unit price info in AssetSummaryVm.
        [Required]
        public virtual string LastUpdate { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public virtual decimal MarketPrice { get; set; }


        

        


        // Added as NH requirement for composite keys: 'composite-id class must override Equals()'.
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var that = (Position)obj;

            return PositionId == that.PositionId && PositionId == that.PositionId;
        }

        public override int GetHashCode() {
            return (PositionId + "|" + PositionId).GetHashCode();
        }


    }
}
