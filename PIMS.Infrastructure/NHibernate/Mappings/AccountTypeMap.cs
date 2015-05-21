using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AccountTypeMap : ClassMap<AccountType>
    {
        public AccountTypeMap()
        {
            Table("AccountType");
            Id(x => x.KeyId, "AccountTypeId");
  
            Map(x => x.AccountTypeDesc, "AccountType");
            Map(x => x.Url, "Url");
            
            // 1:M 
            // An AccountType can be associated with many Positions on many Assets.
            HasMany(x => x.Positions)
                .KeyColumn("PositionAccountTypeId") // added 4-14-15
                .Inverse();


        }

    }
}
