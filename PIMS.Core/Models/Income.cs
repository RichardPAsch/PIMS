using System;
using System.Collections.Generic;

namespace PIMS.Core.Models
{
    public class Income 
    {
        // Considered as child object of Aggregate root (Asset).

        public virtual Guid IncomeId { get; set; }

        public virtual decimal Actual { get; set; }

        // Projected amount based on asset profile.
        public virtual decimal Projected { get; set; }

        public virtual DateTime? DateRecvd { get; set; }

        public virtual IList<Asset> Security { get; set; }


        //public virtual byte[] Version { get; set; }
    }
}
