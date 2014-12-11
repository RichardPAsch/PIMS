using System;
using System.Collections.Generic;
using System.Linq;


namespace PIMS.Core.Models
{
    public class Asset 
    {
        /* ----------------------------------------------------------------------------
         *  Aggregate ROOT object. Modifications here must also be made to AssetMap.
         * --------------------------- ------------------------------------------------
        */
        
        // Read-only to prevent re-creating collection. Will create empty collection
        // upon class instantiation.
        // Commented: No need to have associated collection with each instance!
        //private readonly IList<Classification> _classifications = new List<Classification>();
        
        
        public virtual string Url { get; set; }

        public virtual Guid AssetId { get; set; }
        
        public virtual Investor Investor { get; set; }
        
        public virtual AssetClass AssetClass { get; set; }     // e.g., common stcck

        public virtual Profile Profile { get; set; }

        // Unique per Investor/Asset/AccountType
        public virtual IList<Position> Positions { get; set; }

        // Unique per Investor/Asset/AccountType
        public virtual IList<Income> Revenue { get; set; }
   
        
       


        // TODO - reevaluate
        // lookup can be provided via link to Bloomberg site:
        // http://www.bloomberg.com/markets/symbolsearch?query=etv&commit=Find+Symbols

        // NHibernate requirement to prevent dirty reads.
        //public virtual byte[] Version { get; set; }

       
        
    }
}
