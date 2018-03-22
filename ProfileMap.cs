﻿using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class ProfileMap : ClassMap<Profile>
    {

        public ProfileMap()
        {
            // Domain object identifier, matches table PK.
            Id(x => x.ProfileId)
                .GeneratedBy
                .GuidComb();

            // TODO: can we get away with not using Url from model?
            Map(x => x.TickerSymbol);
            Map(x => x.TickerDescription);
            Map(x => x.DividendFreq, "DividendFrequency");
            Map(x => x.DividendRate).Precision(5).Scale(4);
            Map(x => x.DividendYield).Precision(6).Scale(4);
            Map(x => x.EarningsPerShare).Precision(6);
            Map(x => x.PE_Ratio,"PERatio").Precision(6).Scale(2);
            Map(x => x.LastUpdate);
            Map(x => x.ExDividendDate);
            Map(x => x.DividendPayDate);
            Map(x => x.Price, "UnitPrice").Precision(7).Scale(4);
            Map(x => x.Url);
            Map(x => x.CreatedBy);

        }
    }
}
