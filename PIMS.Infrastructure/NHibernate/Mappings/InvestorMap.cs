using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class InvestorMap : ClassMap<Investor>
    {
        public InvestorMap() {
            Table("Investor");
            Id(x => x.InvestorId);
            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.Address1);
            Map(x => x.Address2);
            Map(x => x.City);
            Map(x => x.ZipCode);
            Map(x => x.Phone);
            Map(x => x.Mobile);
            Map(x => x.AspNetUsersId);
            Map(x => x.EMailAddr);
            Map(x => x.BirthDay);

            //HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();




        }
    }
}
