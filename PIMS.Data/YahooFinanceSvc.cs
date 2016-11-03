using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Data
{
    public static class YahooFinanceSvc
    {
        public static Profile ProcessYahooProfile(string ticker, Profile profileToCreateOrUpdate )
        {
            try
            {
                using (var web = new WebClient())
                {
                    //ticker = "CSQ,HMC";  // test x Profile-Projections
                    var csvProfile = web.DownloadString("http://finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=nsodyreqr1"); // orig
                    //var csvProfile = web.DownloadString("http://finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=sodyrr1");  // test x Profile-Projections

                    var profile = YahooParser.MapToProfile(csvProfile);

                    // Yahoo returns the following equivalent data upon bad ticker info.
                    if (profile.TickerDescription.Trim().ToUpper() == profile.TickerSymbol.ToUpper().Trim())
                        return null;

                    var updatedProfile = UpdateProfile(profileToCreateOrUpdate, profile);
                    return updatedProfile;
                }
            }
            catch (Exception ex)
            {
                var debug = ex.Message;
                return null;
            }
    
        }


        public static List<ProfileProjectionVm> ProcessYahooProfiles(string[] recvdTickers) {

            try {
                using (var web = new WebClient()) {
                    var yahooUrl = "http://finance.yahoo.com/d/quotes.csv?s=";
                    for (var t = 0; t < recvdTickers.Length; t++) {
                        if (t == 0){
                            yahooUrl += recvdTickers[t];
                        }else{
                            if (!string.IsNullOrEmpty(recvdTickers[t]))
                                yahooUrl += "," + recvdTickers[t];
                            else
                                break;
                        }
                    }

                    yahooUrl += "&f=sodyrr1";
                    var csvProfiles = web.DownloadString(yahooUrl);

                    var profileList = YahooParser.MapToProfileProjection(csvProfiles);
                   
                    return profileList;
                }
            }
            catch (Exception ex) {
                // Either connectivity, or bad input (ticker symbol : 'Input string was not in a correct format') issue.
                return null;
            }

        }


        public static Profile UpdateProfile(Profile recvdProfile, Profile yahooProfile)
        {
            
            // Handle BOTH new and existing Profile info.
            if (recvdProfile.ProfileId == default(Guid))
                recvdProfile.ProfileId = Guid.NewGuid();

            recvdProfile.TickerSymbol = yahooProfile.TickerSymbol.ToUpper().Trim();
            recvdProfile.TickerDescription = yahooProfile.TickerDescription.Trim();

            if (string.IsNullOrWhiteSpace(recvdProfile.DividendFreq))
                recvdProfile.DividendFreq = "TBD";
            if ((recvdProfile.Price == default(int) && yahooProfile.Price != default(int))
                                          || (recvdProfile.Price != default(int) && yahooProfile.Price != default(int)))
                recvdProfile.Price = yahooProfile.Price;
            if ((recvdProfile.DividendYield == default(int) && yahooProfile.DividendYield != default(int))
                                          || (recvdProfile.DividendYield != default(int) && yahooProfile.DividendYield != default(int)))
                recvdProfile.DividendYield = yahooProfile.DividendYield;
            if ((recvdProfile.EarningsPerShare == default(int) && yahooProfile.EarningsPerShare != default(int))
                                         || (recvdProfile.EarningsPerShare != default(int) && yahooProfile.EarningsPerShare != default(int)))
                recvdProfile.EarningsPerShare = yahooProfile.EarningsPerShare;
            if ((recvdProfile.PE_Ratio == default(int) && yahooProfile.PE_Ratio != default(int))
                                         || (recvdProfile.PE_Ratio != default(int) && yahooProfile.PE_Ratio != default(int)))
                recvdProfile.PE_Ratio = yahooProfile.PE_Ratio;

            recvdProfile.LastUpdate = DateTime.Now;

            if ((recvdProfile.ExDividendDate == default(DateTime) || yahooProfile.ExDividendDate.HasValue)
                                         || (recvdProfile.ExDividendDate != default(DateTime) && yahooProfile.ExDividendDate.HasValue))
                recvdProfile.ExDividendDate = yahooProfile.ExDividendDate;


            if ((recvdProfile.DividendPayDate == default(DateTime) && yahooProfile.DividendPayDate.HasValue)
                                         || (recvdProfile.DividendPayDate != default(DateTime) && yahooProfile.DividendPayDate.HasValue))
                recvdProfile.DividendPayDate = yahooProfile.DividendPayDate;
                //recvdProfile.DividendPayDate = CheckDateRelationship(ReformatDate(yahooProfile.DividendPayDate));

                
            return recvdProfile;
        }


        private static string ReformatDate(string dateToParse)
        {
            // Ignore default Yahoo escape chars in date fields.
            var newDate = dateToParse.Where(c => c != (char) 34).Aggregate(string.Empty, (current, c) => current + c);
            return Convert.ToDateTime(newDate).ToString("dd").Trim() + "-" + Convert.ToDateTime(newDate).ToString("MMM").Trim();
        }


        private static string CheckDateRelationship(string yahooDate)
        {
            // Supply correct year for missing year portion of Yahoo value.
            if (Convert.ToDateTime(yahooDate) < DateTime.Today)
                yahooDate = Convert.ToDateTime(yahooDate).AddYears(1).ToString("d");

            return yahooDate;
        }


    }

}
