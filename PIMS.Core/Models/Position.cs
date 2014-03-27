using System;
using System.Collections.Generic;

namespace PIMS.Core.Models
{
    public class Position
    {
        // Considered as child object of Aggregate root (Asset).

        // Current market unit price
        public virtual decimal MarketPrice { get; set; }

        public virtual Guid PositionId { get; set; }

        public virtual DateTime PurchaseDate { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual decimal TotalValue { get; set; }

        // Cost basis unit price
        public virtual decimal UnitPrice { get; set; }

        public virtual IList<Asset> Security { get; set; }

        // NHibernate requirement to prevent dirty reads.
        //public virtual byte[] Version { get; set; }
    }
}
