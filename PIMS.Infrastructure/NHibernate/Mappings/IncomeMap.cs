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
 

            // References other side (many) of Asset NH 1:M relationship.
            References(x => x.IncomeAsset)   // NH mapping for '1' side of relation to Asset.
                .Column("IncomeAssetId")     // NH FK column in Income table.
                .Not.Update()
                .Not.Insert();

           
        }
    }
}
