using System;
using System.Linq;
using PIMS.Core.Models;


namespace PIMS.Data
{
    public static class YahooParser
    {
        public static Profile MapToProfile(string csvData)
        {
            Profile profile = null;
            var rows = csvData.Replace("\r", "").Split('\n');

            foreach (var newProfile in 
                    from row in rows 
                    where !string.IsNullOrEmpty(row) 
                    select row.Split(',') into cols 
                    select new Profile
                                    {
                                        ProfileId = Guid.NewGuid(),
                                        TickerSymbol = cols[1].Replace("\"", ""),
                                        TickerDescription = cols[0].Replace("\"", ""),
                                        DividendFreq = "M", //TODO - calculate based on payment history?
                                        DividendRate = cols[3] == "N/A" ? 0 : Convert.ToDecimal(cols[3]),
                                        EarningsPerShare = cols[6] == "N/A" ? 0 : Convert.ToDecimal(cols[6]),
                                        SharePrice = cols[2] == "N/A" ? 0 : Convert.ToDecimal(cols[2]),
                                        DividendYield = cols[4] == "N/A" ? 0 : Convert.ToDecimal(cols[4]),
                                        PE_Ratio = cols[5] == "N/A" ? 0 : Convert.ToDecimal(cols[5]),
                                        LastUpdate = DateTime.Now
                                    }
                    )
              
      
            {
                profile = newProfile;
            }

            return profile;
        }

    }


}
