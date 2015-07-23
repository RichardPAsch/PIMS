using System;
using System.Collections.Generic;


namespace PIMS.Core.Models
{
    public class Asset 
    {
        /* ----------------------------------------------------------------------------
         *  Aggregate ROOT object. Modifications here must also be made to AssetMap.
         * --------------------------- ------------------------------------------------
        */
        
        public virtual string Url { get; set; }

        // NH FK mapping - IncomeAssetId
        public virtual Guid AssetId { get; set; }

        // NH FK mapping - Asset - AssetClassId
        public virtual Guid AssetClassId { get; set; }

        // 'Many' side for NH mapping re: M:M relationship.
        public virtual IList<Investor> Investors { get; set; }

        // Unique per Investor/Asset/AccountType e.g., Roth-IRA
        public virtual IList<Position> Positions { get; set; }

        // For NH mapping for 'many' side of 1:M 
        public virtual IList<Income> Revenue { get; set; }




        // Referenced by domain.
        public virtual Guid InvestorId { get; set; }
        
        public virtual Investor Investor { get; set; }
        
        public virtual AssetClass AssetClass { get; set; }     // e.g., common stcck

        public virtual Profile Profile { get; set; }

        public virtual DateTime? LastUpdate { get; set; }

        public virtual Guid ProfileId { get; set; }
        

        // TODO - implement vs. Asset deletion ?
        // public virtual bool IsActive {get; set;}      

        
    }
}
