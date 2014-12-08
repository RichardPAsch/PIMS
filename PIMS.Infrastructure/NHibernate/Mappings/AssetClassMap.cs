using FluentNHibernate.Mapping;
using PIMS.Core.Models;

namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AssetClassMap : ClassMap<AssetClass>
    {
        public AssetClassMap()
        {
            Id(x => x.KeyId,"AssetClassId");
            Map(x => x.Code);
            Map(x => x.Description);
            Table("AssetClass");

        }
       
    }
}
