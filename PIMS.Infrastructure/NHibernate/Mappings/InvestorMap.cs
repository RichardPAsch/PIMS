using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class InvestorMap : ClassMap<Investor>
    {
        public InvestorMap()
        {
            Table("Investor");
            Id(x => x.InvestorId)
                .GeneratedBy
                .GuidComb();

            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.Address1);
            Map(x => x.Address2);
            Map(x => x.MiddleInitial);
            Map(x => x.City);
            Map(x => x.State);
            Map(x => x.ZipCode);
            Map(x => x.Phone);
            Map(x => x.Mobile);
            Map(x => x.AspNetUsersId);
            Map(x => x.EMailAddr);
            Map(x => x.BirthDay);
            Map(x => x.DateAdded, "LastUpdate");
            Map(x => x.Url, "Link");
            

            // NH required configuration for M:M table configuration.
            HasManyToMany(x => x.Assets)
                .Cascade.All()
                .ParentKeyColumn("InvestorId")
                .ChildKeyColumn("AssetId")
                .Inverse() // 'Asset' responsible for saving
                .Table("AssetInvestor");


            // Added 6-12-15 - NH mapping support for Investor object.
            HasMany(x => x.InvestorAssets)
                .Table("Investor")
                .KeyColumn("InvestorId");

        }
    }
}
