using System;
using System.Collections.Generic;
using System.Linq;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Data
{
    internal static class YahooParser
    {
        public static Profile MapToProfile(string csvData)
        {
            Profile profile = null;
            var rows = csvData.Replace("\r", "").Split('\n');

            // Trap for ticker description containing a comma.
            if (rows[0].Split(',').Length >= 10)
                rows[0] = RebuildTickerDesc(rows[0].Split(',')); 
            
                // Yahoo params: &f=n s b2 dyreqr1
                foreach (var newProfile in 
                    from row in rows
                    where !string.IsNullOrEmpty(row)
                    select row.Split(',')
                    into cols
                    select new Profile
                           {
                               AssetId = Guid.NewGuid(),
                               TickerDescription = cols[0].Replace("\"", ""),
                               TickerSymbol = cols[1].Replace("\"", ""),
                               Price = cols[2] == "N/A" ? 0 : Convert.ToDecimal(cols[2]),
                               DividendRate = cols[3] == "N/A" ? 0 : Convert.ToDecimal(cols[3]),
                               DividendYield = cols[4] == "N/A" ? 0 : Convert.ToDecimal(cols[4]),
                               PE_Ratio = cols[5] == "N/A" ? 0 : Convert.ToDecimal(cols[5]),
                               EarningsPerShare = cols[6] == "N/A" ? 0 : Convert.ToDecimal(cols[6]),
                               ExDividendDate =
                                   cols[7] == "N/A"
                                       ? new DateTime(1900, 1, 1)
                                       : DateTime.Parse(BuildDateString(cols[7])),
                               DividendPayDate =
                                   cols[8] == "N/A"
                                       ? new DateTime(1900, 1, 1)
                                       : DateTime.Parse(BuildDateString(cols[8])),
                               LastUpdate = DateTime.Now,
                               DividendFreq = "TBD",
                           }
                    )


                {
                    profile = newProfile;
                }

                return profile;
            }


        public static List<ProfileProjectionVm> MapToProfileProjection(string csvProfilesToMap)
        {
            var profileRows = csvProfilesToMap.Replace("\r", "").Split('\n');
            // Remove empty last array element due to trailing \n in received string.
            profileRows = profileRows.Take(profileRows.Count() - 1).ToArray(); 

            var profilesModel = profileRows.Select(profile => profile.Split(','))
                              .Select(currentProfileData => new ProfileProjectionVm
                                                            {
                                                                Ticker = currentProfileData[0], 
                                                                Capital = 0, 
                                                                Price = currentProfileData[1] == "" 
                                                                            ? 0 
                                                                            : Convert.ToDecimal(currentProfileData[1]), 
                                                                DivRate = currentProfileData[2] == "N/A" 
                                                                            ? 0 
                                                                            : Convert.ToDecimal(currentProfileData[2]), 
                                                                DivYield = currentProfileData[3] == "N/A" 
                                                                            ? "0" 
                                                                            : currentProfileData[3], 
                                                                PE_Ratio = currentProfileData[4] == "N/A" 
                                                                            ? "0" 
                                                                            : currentProfileData[4], 
                                                                DivDate = currentProfileData[5] == "N/A" 
                                                                            ? new DateTime(1900, 1, 1) 
                                                                            : DateTime.Parse(BuildDateString(currentProfileData[5])), 
                                                                ProjectedRevenue = 0
                                                            })
                              .ToList();

            return new List<ProfileProjectionVm>(profilesModel.OrderBy(p => p.Ticker));
        } 





        // A hack for unsuccessful use of Replace(), etc. in supressing escape chars in CSV data.
        private static string BuildDateString (string sourceDate)
        {
            var targetDate = string.Empty;
            for (var i = 0; i < sourceDate.Length; i++)
            {
                if (sourceDate.Substring(i, 1) != "\"" && i != 0)
                    targetDate += sourceDate.Substring(i, 1);
            }

            return targetDate;
        }


        private static string RebuildTickerDesc (IList<string> index0DataRecvd)
        {
            var newTickerDesc = index0DataRecvd[0] + " " + index0DataRecvd[1];
            var newString = newTickerDesc + ","
                                          + index0DataRecvd[2] + ","
                                          + index0DataRecvd[3] + ","
                                          + index0DataRecvd[4] + ","
                                          + index0DataRecvd[5] + ","
                                          + index0DataRecvd[6] + ","
                                          + index0DataRecvd[7] + ","
                                          + index0DataRecvd[8] + ","
                                          + index0DataRecvd[9];
            return newString;
        }



    }


}

