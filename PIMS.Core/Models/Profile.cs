using System;
using System.Collections.Generic;

namespace PIMS.Core.Models
{
    public class Profile
    {
        // Not considered as part of Aggregate root (Asset).

        public virtual Guid ProfileId { get; set; }

        public virtual string TickerSymbol { get; set; }

        public virtual string TickerDescription { get; set; }

        public virtual string DividendFreq { get; set; }

        public virtual decimal? DividendRate { get; set; }

        public virtual decimal EarningsPerShare { get; set; }

        public virtual decimal SharePrice { get; set; }

        public virtual decimal? DividendYield { get; set; }

        public virtual IList<Asset> Security { get; set; }

        // ReSharper disable once InconsistentNaming
        public virtual decimal? PE_Ratio { get; set; }

        public virtual DateTime LastUpdate { get; set; }

    }
}
