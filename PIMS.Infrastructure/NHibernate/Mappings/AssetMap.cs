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
            // that happens to have only 1 entity (Income).
            References(x => x.Income, "IncomeId");
            

            // Profile is NOT in context of User.
            References(x => x.Profile, "ProfileId");


            // Position is in context of User for any given Asset. Collection
            // that happens to have only 1 entity (Position).
            References(x => x.Position, "PositionId");
            

            // User is in context of an Asset
            References(x => x.User, "UserId");

            
            References(x => x.AccountType);


            //// 1:1 relationship; Classification is in context of Asset.
            //Component(x => x.Classification, m => {
            //                            m.Map(x => x.Code);
            //                            m.Map(x => x.Description);
            //                        }

            //    );






        }
    }
}
