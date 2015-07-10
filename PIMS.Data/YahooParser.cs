using System;
using System.Linq;
using PIMS.Core.Models;


namespace PIMS.Data
{
    internal static class YahooParser
    {
        public static Profile MapToProfile(string csvData)
        {
            Profile profile = null;
            var rows = csvData.Replace("\r", "").Split('\n');

            // Yahoo params: &f=n s b2 dyreqr1
            foreach (var newProfile in 
                    from row in rows 
                    where !string.IsNullOrEmpty(row) 
                    select row.Split(',') into cols 
                    select new Profile
                                    {


                                        AssetId           = Guid.NewGuid(),
                                        TickerDescription = cols[0].Replace("\"", ""),
                                        TickerSymbol      = cols[1].Replace("\"", ""),
                                        Price             = cols[2] == "N/A" ? 0 : Convert.ToDecimal(cols[2]),
                                        DividendRate      = cols[3] == "N/A" ? 0 : Convert.ToDecimal(cols[3]),
                                        DividendYield     = cols[4] == "N/A" ? 0 : Convert.ToDecimal(cols[4]),
                                        PE_Ratio          = cols[5] == "N/A" ? 0 : Convert.ToDecimal(cols[5]),
                                        EarningsPerShare  = cols[6] == "N/A" ? 0 : Convert.ToDecimal(cols[6]),
                                        ExDividendDate    = cols[7] == "N/A" ? new DateTime(1900, 1, 1) : DateTime.Parse(cols[7]),
                                        DividendPayDate = cols[8] == "N/A" ? new DateTime(1900, 1, 1) : DateTime.Parse(cols[8]),
                                        LastUpdate        = DateTime.Now,
                                        DividendFreq      = "TBD", //TODO - calculate based on payment history?
                                    }
                    )
              
      
            {
                profile = newProfile;
            }

            return profile;
        }

    }


}
