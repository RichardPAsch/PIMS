using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class PositionMap : ClassMap<Position>
    {
        // Superceded by component mapping in AssetMap.cs
        public PositionMap()
        {
            // Domain object identifier, matches table PK.
            Id(x => x.PositionId);
            Map(x => x.PurchaseDate);
            Map(x => x.Quantity);
            Map(x => x.UnitCost).Precision(6);
            Map(x => x.Account);
            Map(x => x.LastUpdate, "LastUpdate");

            // Bypass NH default and load this child aggregate entity.
            //Not.LazyLoad();
            HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();
        }
    }
}
