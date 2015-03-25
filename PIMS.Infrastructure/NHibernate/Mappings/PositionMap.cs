using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class PositionMap : ClassMap<Position>
    {
        public PositionMap()
        {
            Table("Position");
            Id(x => x.PositionId)
                .GeneratedBy
                .GuidComb();

            Map(x => x.PurchaseDate);
            Map(x => x.Quantity);
            Map(x => x.LastUpdate);
            Map(x => x.MarketPrice,"UnitCost").Precision(6).Scale(2);
            Map(x => x.AcctTypeId, "PositionAccountTypeId");
            Map(x => x.PositionAssetId);
            

            // M:1
            // One or more Positions may be associated with an Asset.
            // ** Avoid cryptic NH "Invalid index n for this SqlParameterCollection with Count = n" error,
            // via Insert() & Update(). Applicable to FK exposure in M:1 relationships. **
            References(x => x.PositionAsset)    // references 'one' side of Asset/Position relationship.
                 .Column("PositionAssetId")     // FK 
                 .Not.Update()
                 .Not.Insert();



            // Bypass NH default and load this child aggregate entity.
            //Not.LazyLoad();
            
        }
    }
}
