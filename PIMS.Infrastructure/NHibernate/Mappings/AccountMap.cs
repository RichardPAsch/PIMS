using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AccountMap : ClassMap<Account>
    {
        public AccountMap() {
            Table("Account");
            Id(x => x.Id);
            Map(x => x.AccountPassword, "UserPassword");
            Map(x => x.AccountPasswordConfirm, "UserPasswordConfirm");
            Map(x => x.AccountName, "UserName");

        }
    }
}
