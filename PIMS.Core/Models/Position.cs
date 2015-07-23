using System;
using System.Collections.Generic;
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
        public virtual Guid PositionId { get; set; }

        // NH - 'one' side of Position/AccountType rel.
        public virtual AccountType Account { get; set; }

        //TODO: Superflouous - Income is associated with Asset already.
        // NH - 'many' side of Position/Income rel.
        //public virtual Income PositionIncome { get; set; }
        //public virtual IList<Income> PositionIncomes { get; set; } // added 7-10-15

        // NH - FK ref. 
        public virtual Guid AcctTypeId { get; set; }



        public virtual string InvestorKey { get; set; }
        
        [Required]
        public virtual DateTime PurchaseDate { get; set; }

        [Required]
        [Range(1,10000)]
        public virtual int Quantity { get; set; }
        
        // Useful for most recent unit price info in AssetSummaryVm.
        [Required]
        public virtual DateTime LastUpdate { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public virtual decimal MarketPrice { get; set; }

        [Required]
        public virtual string TickerSymbol { get; set; }



    }
}
