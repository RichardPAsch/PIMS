using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AssetClassMap : ClassMap<AssetClass>
    {
        public AssetClassMap()
        {
            Table("AssetClassification");
            Id(x => x.KeyId,"AssetClassId")
                .GeneratedBy
                .GuidComb();

            Map(x => x.Url, "Link");
            Map(x => x.Code);
            Map(x => x.Description);

        }
       
    }
}
