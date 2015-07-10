using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class IncomeMap : ClassMap<Income>
    {
        public IncomeMap()
        {
            Id(i => i.IncomeId)
                .GeneratedBy
                .GuidComb();

            Map(x => x.Actual).Precision(6);
            Map(x => x.Projected).Precision(6);
            Map(x => x.DateRecvd, "DateReceived");
            Map(x => x.LastUpdate);
            Map(x => x.AssetId, "IncomeAssetId");
            Map(x => x.IncomePositionId); // added 5-27-15
            Map(x => x.Url);


            // M:1
            // One or more Income records may be associated with an Asset.
            // References other side (one) of Asset NH 1:M relationship.
            References(x => x.IncomeAsset)   // NH mapping for '1' side of relation to Asset.
                .Column("IncomeAssetId")     // NH FK column in Income table.
                .Not.Update()
                .Not.Insert();

            // M:1
            // One or more Income records may be associated with a Position.
            // References other side (many) of Position NH 1:M relationship.
            References(x => x.IncomePosition)   // NH mapping for '1' side of relation to Position.
                .Column("IncomePositionId")     // NH FK column in Income table.
                .Not.Update()
                .Not.Insert();

          
           
        }
    }
}
