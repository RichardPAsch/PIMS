using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class IncomeMap : ClassMap<Income>
    {
        public IncomeMap() {

            Id(x => x.AssetId);
            Map(x => x.Actual).Precision(6);
            Map(x => x.Projected).Precision(6);
            Map(x => x.DateRecvd, "DateReceived");
            //Not.LazyLoad();

            // Creates other side (many) of Asset NH 1:M relationship.
            // Cascade from Asset to these Income object(s).
            // DeleteOrphan() - prevents any orphaned records + provides cascade saves, updates, & deletes.
            // Inverse() - save responsibility to parent (Asset).
            //HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();

        }
    }
}
