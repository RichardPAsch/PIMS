using System.Linq;
using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class AssetMap : ClassMap<Asset>
    {
        /// <summary>
        /// 11-14-2013
        /// Although relationships with composite domain objects, i.e., Income, and Position, are 1:1 here, it is documented
        /// that true 1:1 associations rarely exists, and that NHibernate mapping configuration at this point at least, using
        /// 'HasOne' mapping, proves problematic and error prone. Therefore, we'll reference these child objects via the 
        /// use of 'References' -- although this is defined for use as in M:1 relationship.
        /// 
        /// Asset is an aggregate root Entity object. Income and Position as well are both Entity objects, while Profile is a
        /// Value object type.
        /// </summary>

        public AssetMap()
        {
            // Domain object identifier, matches table PK.
            Id(x => x.AssetId);


            // Income is in context of User for any given Asset. Collection
            // happens to have only 1 entity (Income).
            //References(x => x.Income, "IncomeId");

            // AssetClass is in context of an Asset
            References(x => x.AssetClass);
            

            // Profile is NOT in context of User.
            References(x => x.Profile, "ProfileId");


            // Position is in context of Investor for any given Asset. Collection
            // happens to have only 1 entity (Position).
            //References(x => x.Positions, "PositionId");
            

            // Investor is in context of an Asset
            References(x => x.Investor, "InvestorId");

            // AccountType is in context of an Position
            //Component(x => x.Positions);
            //References(x => x.Positions.First().Account, "AccountTypeId");
            //Map(x => x.AccountType);


            // 1:many relationship; Position is in context of Asset.
            // 11-22-14: NUnit test Error - NHibernate.MappingException : Could not determine type for: PIMS.Core.Models.AccountType, 
            //           PIMS.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null, for columns: NHibernate.Mapping.Column(Account)
            //Component(x => x.Revenue.FirstOrDefault(), m =>
            //                                                {
            //                                                    m.Map(x => x.Quantity);
            //                                                    m.Map(x => x.PurchaseDate);
            //                                                    m.Map(x => x.UnitCost);
            //                                                    m.Map(x => x.Url);
            //                                                    Component(x => x.Revenue.FirstOrDefault().Account,
            //                                                        a => a.Map(x => x.AccountTypeDesc));
            //                                                }

            //        );






        }
    }
}
