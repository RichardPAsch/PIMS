﻿using FluentNHibernate.Mapping;
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
            Map(x => x.UnitPrice).Precision(6);
            Map(x => x.TotalValue).Precision(10);
            Map(x => x.MarketPrice).Precision(6);
            // Bypass NH default and load this child aggregate entity.
            //Not.LazyLoad();
            HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();
        }
    }
}
