using System.Net;
using Newtonsoft.Json;


namespace PIMS.Data
{
    internal static class YahooFinanceSvc
    {
        public static string Process(string ticker)
        {
            string csvProfile;

            using (var web = new WebClient())
            {
                csvProfile = web.DownloadString("http://finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=nsb2dyre");
            }

            var profile = YahooParser.MapToProfile(csvProfile);
          
           return JsonConvert.SerializeObject(profile);

        }


    }

}
