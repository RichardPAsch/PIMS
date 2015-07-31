using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class PositionMap : ClassMap<Position>
    {
        public PositionMap()
        {
            Table("Position");
            Id(x => x.PositionId, "PositionId")
                .GeneratedBy
                .GuidComb();

            Map(x => x.PositionAssetId);
            Map(x => x.AcctTypeId, "PositionAccountTypeId");
            Map(x => x.PurchaseDate);
            Map(x => x.Quantity);
            Map(x => x.MarketPrice, "UnitCost").Precision(6).Scale(2);
            Map(x => x.LastUpdate);
            Map(x => x.InvestorKey); // added 4-14-15
            Map(x => x.Url);
            
            

            // M:1
            // One or more Positions may be associated with an Asset.
            // ** Avoid cryptic NH "Invalid index n for this SqlParameterCollection with Count = n" error,
            // via Insert() & Update(). Applicable to FK exposure in M:1 relationships. **
            References(x => x.PositionAsset)    // references 'one' side of Asset/Position relationship.
                 .Column("PositionAssetId")     // FK 
                 .Not.Update()
                 .Not.Insert();


            // M:1
            // One or more Positions on one or more Assets may be associated with an AccountType.
            References(x => x.Account)
                .Column("PositionAccountTypeId")
                .Not.Update()
                .Not.Insert();

            HasMany(x => x.PositionIncomes)  
                .KeyColumn("PositionId");

        }
    }
}
