using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Profile
    {
        // Not considered as part of Aggregate root (Asset).
        public virtual string Url { get; set; }

        // NH relational mapping for 'many' side of M:1 Asset/Profile.
        //public virtual IList<Asset> AssetProfile { get; set; }

        [Required]
        public virtual Guid ProfileId { get; set; }

        //TODO: remove code references before deleting.
        [Required]
        public virtual Guid AssetId { get; set; }

        [Required]
        public virtual string TickerSymbol { get; set; }

        [Required]
        public virtual string TickerDescription { get; set; }

        public virtual string DividendFreq { get; set; }

        [Required]
        [Range(1.00, 40.00)]
        public virtual decimal DividendRate { get; set; }

        [Required]
        [Range(0.01, 50.00)]
        public virtual decimal? DividendYield { get; set; }

        public virtual decimal EarningsPerShare { get; set; }

        // ReSharper disable once InconsistentNaming
        public virtual decimal? PE_Ratio { get; set; }

        [Required]
        public virtual DateTime LastUpdate { get; set; }

        public virtual DateTime? ExDividendDate { get; set; }

        public virtual DateTime? DividendPayDate { get; set; }

        // aka 'Ask Price' or todays' market price.
        public virtual decimal Price { get; set; } 

    }
}
