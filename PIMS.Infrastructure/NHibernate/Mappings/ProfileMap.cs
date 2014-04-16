using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class ProfileMap : ClassMap<Profile>
    {

        public ProfileMap()
        {
            // Domain object identifier, matches table PK.
            Id(x => x.ProfileId);
            Map(x => x.TickerSymbol);
            Map(x => x.TickerDescription);
            Map(x => x.DividendYield).Precision(4);
            Map(x => x.DividendRate).Precision(5);
            Map(x => x.DividendFreq,"DividendFrequency");
            Map(x => x.PE_Ratio,"PERatio").Precision(4);
            Map(x => x.EarningsPerShare).Precision(2);
            Map(x => x.SharePrice).Precision(2);
            Map(x => x.LastUpdate);


            HasMany(x => x.Security).Cascade.DeleteOrphan().Inverse();


        }
    }
}
