using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("Users");
            Id(x => x.UserId);
            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.Password);
            Map(x => x.UserName);
            Map(x => x.EMail);

            HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();

        }
    }
}
