﻿using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AssetMap : ClassMap<Asset>
    {
        /// <summary>
        /// 11-14-2013
        /// Although relationships with composite domain objects, i.e., Income, and Position, are 1:1 here, it is documented
        /// that true 1:1 associations rarely exists, and that NHibernate mapping configuration at this point at least, using
        /// 'HasOne' mapping, proves problematic and error prone. Therefore, we'll reference these child objects via the 
        /// use of 'References' -- although this is defined for use as in M:1 relationship.
        /// 
        /// Asset is an aggregate root Entity object. Income and Position as well are both Entity objects, while Profile is a
        /// Value object type.
        /// </summary>

        public AssetMap()
        {
            // Domain object identifier, matches table PK.
            Table("Asset");
            Id(x => x.AssetId)
                .GeneratedBy
                .GuidComb();

            Map(x => x.InvestorId, "AssetInvestorId");

            HasManyToMany(x => x.Investors)
                .Table("AssetInvestor")
                .ParentKeyColumn("AssetId")             // always current table containing this mapping
                .ChildKeyColumn("InvestorId");          // table containing records for insertion into IList.


            // 1:M - Each Asset may have one or more Positions.
            HasMany(x => x.Positions)
                .Table("Position")
                .KeyColumns.Add("PositionAssetId")       // references Position FK
                .Cascade
                .AllDeleteOrphan();


            // 1:M
            // Each Asset may have one or more Income items (Revenue).
            HasMany(x => x.Revenue)
                .Table("Income")
                .KeyColumns.Add("IncomeAssetId")    // references FK in Income
                .Cascade                            // cascade Income saves, updates, & deletes.
                .AllDeleteOrphan();                 // prevents any orphaned records 



            // M:1
            References(x => x.AssetClass)
                .Column("AssetClassId")
                .Not.Update()
                .Not.Insert();

            // NH FK reference.
            Map(x => x.AssetClassId);
                


            //  NH FK reference regarding M:1
            References(x => x.Profile)
                .Column("ProfileId");
            
        }
    }
}
