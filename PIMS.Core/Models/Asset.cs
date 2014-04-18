using System;

namespace PIMS.Core.Models
{
    public class Asset 
    {
        /* ---------------------------
         *  Aggregate ROOT object.
         * --------------------------- 
        */
        
        // Read-only to prevent re-creating collection. Will create empty collection
        // upon class instantiation.
        // Commented: No need to have associated collection with each instance!
        //private readonly IList<Classification> _classifications = new List<Classification>();
        
        // Lookup data for assigning an asset class.
        //public virtual IList<Classification> Classifications {
          
        //    get { return _classifications; }
        //}
        

        public virtual Guid AssetId { get; set; }

        public virtual AssetClass AssetClass { get; set; }
        
        public virtual Income Income { get; set; }

        public virtual Position Position { get; set; }

        public virtual Profile Profile { get; set; }

        public virtual User User { get; set; }

        public virtual string AccountType { get; set; }   // e.g. Roth-IRA, IRA, etc.


        // TODO - reevaluate
        // lookup can be provided via link to Bloomberg site:
        // http://www.bloomberg.com/markets/symbolsearch?query=etv&commit=Find+Symbols

        // NHibernate requirement to prevent dirty reads.
        //public virtual byte[] Version { get; set; }

       
        
    }
}
