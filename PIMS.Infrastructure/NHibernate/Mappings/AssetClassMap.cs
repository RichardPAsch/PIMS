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

            // 1:M 
            // An AssetClassifcation can be associated with many Assets.
            HasMany(x => x.AssetClassifications)
                .Table("AssetClassification")
                .KeyColumn("AssetClassId")
                .Cascade
                .All();



        }
       
    }
}
