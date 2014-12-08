using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AccountTypeMap : ClassMap<AccountType>
    {
        public AccountTypeMap()
        {
            Id(x => x.KeyId, "AccountTypeId");
            Map(x => x.AccountTypeDesc, "AccountType");
            Table("AccountType");

        }

    }
}
