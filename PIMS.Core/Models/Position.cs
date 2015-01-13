using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    public class Position
    {
        // Considered as child object of Aggregate root (Asset).
        public virtual string Url { get; set; }

        public virtual string InvestorKey { get; set; }

        [Required]
        public virtual Guid PositionId { get; set; }

        [Required]
        public virtual string PurchaseDate { get; set; }

        [Required]
        [Range(1,10000)]
        public virtual int Quantity { get; set; }


        public virtual AccountType Account { get; set; }

        // Useful for most recent unit price info in AssetSummaryVm.
        [Required]
        public virtual string LastUpdate { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public virtual decimal MarketPrice { get; set; }

        public virtual IList<Asset> Security { get; set; }

        // NHibernate requirement to prevent dirty reads.
        //public virtual byte[] Version { get; set; }
    }
}
