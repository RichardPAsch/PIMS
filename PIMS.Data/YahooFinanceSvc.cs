using System;
using System.Linq;
using System.Net;
using PIMS.Core.Models;


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
                    var csvProfile = web.DownloadString("http://finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=nsodyreqr1");

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
