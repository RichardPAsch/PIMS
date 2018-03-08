using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using PIMS.Core.Models;
using PIMS.Data.Repositories;
using PIMS.Core.Models.ViewModels;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{

    /* Business Rules:
     * GET    - All currently available persisted Profiles will be available for grid viewing to allow for comparisons. Additionally,
     *          database searches will be utilized before accessing any specific ticker Profile data via 3rd party financial services.
     * POST   - Data to be persisted will either be 1) currently existing & current (via GET), or 2) non-existent and created (via service).  
     * PUT    - N/A; read-only data refreshed during GET.
     * DELETE - N/A; No business need.
    */


    [RoutePrefix("api/Profile")]
    public class ProfileController : ApiController
    {

        private static IGenericRepository<Profile> _repository;
        private const string BaseTiingoUrl = "https://api.tiingo.com/tiingo/daily/";
        private const string TiingoAccountToken = "95cff258ce493ec51fd10798b3e7f0657ee37740";
        private DateTime cutOffDateTimeForProfileUpdate = DateTime.Now.AddHours(-72);


        public ProfileController(IGenericRepository<Profile> repository)
        {
            _repository = repository;
        }


        [HttpGet]
        [Route("persisted/{tickerSymbol}")]
        public async Task<IHttpActionResult> GetPersistedProfileByTicker(string tickerSymbol)
        {

            var savedProfile = await Task.FromResult(_repository.RetreiveAll()
                                         .Where(p => p.TickerSymbol == tickerSymbol)
                                         .AsQueryable()
                                         .Select(p => p).First());

            if(savedProfile == null)
                return BadRequest("No saved Profile found, or unable to retreive data for ticker: " + tickerSymbol);

            return Ok(savedProfile);
        }



        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllPersistedProfiles()
        {
            // Includes ALL Profiles, even those missing some info.
            var savedProfiles = await Task.FromResult(_repository.RetreiveAll()
                                                                 .OrderBy(p => p.TickerSymbol)
                                                                 .AsQueryable());

            if (savedProfiles == null) return BadRequest("No saved Profiles found, or unable to retreive data.");

            // Use of Vm mandated by received Http status:500 Error - "An error has occurred.","exceptionMessage":"Error getting value
            // from 'DefaultValue' on 'NHibernate.Type.DateTimeOffsetType'.","exceptionType":"Newtonsoft.Json.JsonSerializationException"
            // Proxy setup by NH results in serialization error, although no DateTime-related types exist in projects.
            IList<ProfileVm> profileListing = savedProfiles.Select(p => new ProfileVm
                                                                        {
                                                                            ProfileId = p.ProfileId,
                                                                            TickerSymbol = p.TickerSymbol,
                                                                            TickerDescription = p.TickerDescription,
                                                                            DividendFreq = p.DividendFreq,
                                                                            DividendRate = p.DividendRate,
                                                                            DividendYield = p.DividendYield,
                                                                            EarningsPerShare = p.EarningsPerShare,
                                                                            PE_Ratio = p.PE_Ratio,
                                                                            LastUpdate = p.LastUpdate,
                                                                            ExDividendDate = p.ExDividendDate,
                                                                            DividendPayDate = p.DividendPayDate,
                                                                            Price = p.Price
                                                                        }).ToList();

            if(profileListing.Count >= 1)
                return Ok(profileListing);

            return BadRequest(string.Format("Error fetching saved Profile data"));
        }


        [HttpGet]
        [Route("~/api/Profiles/{t1}/{t2?}/{t3?}/{t4?}/{t5?}")]
        // e.g. http://localhost/Pims.Web.Api/api/Profiles/{t1}/{t2?}/{t3?}/{t4?}/{t5?}/ 
        // t = ticker symbol
        public async Task<IHttpActionResult> GetProfiles(string t1, string t2 = "", string t3 = "", string t4 = "", string t5 = "") {

            if (string.IsNullOrEmpty(t1))
                return BadRequest(string.Format("Error fetching Profile, minimum of 1 ticker symbol required."));

            var tickersTemp = string.Empty;
            tickersTemp += t1;
            if (!string.IsNullOrEmpty(t2)) tickersTemp += "," + t2;
            if (!string.IsNullOrEmpty(t3)) tickersTemp += "," + t3;
            if (!string.IsNullOrEmpty(t4)) tickersTemp += "," + t4;
            if (!string.IsNullOrEmpty(t5)) tickersTemp += "," + t5;

            var tickers = tickersTemp.Split(',');

            var profileProjections = new List<ProfileProjectionVm>();
            foreach (var ticker in tickers)
            {
                try
                {
                    var actionResult = await GetProfileByTicker(ticker) as OkNegotiatedContentResult<Profile>;
                    if (actionResult == null) continue;
                    var profileContent = actionResult.Content;
                    var projectionVm = new ProfileProjectionVm
                                       {
                                           Ticker = profileContent.TickerSymbol.ToUpper().Trim(),
                                           Capital = 0,
                                           ProjectedRevenue = 0,
                                           Price = profileContent.Price,
                                           DivRate = profileContent.DividendRate > 0 ? profileContent.DividendRate : 0
                                       };

                    profileProjections.Add(projectionVm);
                }
                catch (Exception e)
                {
                    return BadRequest("Error obtaining Profile data for \n\'projections\' calculations. Check ticker accuracy.");
                }
            }


            return profileProjections.Count > 0 ?  Ok(profileProjections.ToArray()) : null;

        }



        /*  ** NOTE **
            11.9.2017 -  GetProfileByTicker() & GetProfiles() are now OBSOLETE due to the shutdown of Yahoo Finance API.
                         New ticker profile data now supplied via Tiingo.com API.
            11.10.2017 - Seperate API calls will be necessary for gathering meta & price data, as mandated by Tiingo.
        */

        //[HttpGet]
        //[Route("{tickerForProfile?}")]
        //// e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        //public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        //{
        //    Profile updatedOrNewProfile;
        //    var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

        //    // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
        //    if (existingProfile.Any())
        //    {
        //        // Update Profile table only IF existing Profile was last updated > 24hrs ago; return updated Profile.
        //        if (Convert.ToDateTime(existingProfile.First().LastUpdate) < DateTime.UtcNow.AddHours(-24))
        //        {
        //            updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), existingProfile.First()));
        //            if (updatedOrNewProfile != null)
        //                return Ok(updatedOrNewProfile);
        //        }
        //        else
        //        {
        //            // Return existing table Profile
        //            return Ok(existingProfile.First());
        //        }
        //    }
        //    else
        //    {
        //        // Return new Profile.
        //        updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), new Profile()));
        //        if (updatedOrNewProfile != null)
        //            return Ok(updatedOrNewProfile);
        //    }

        //    return BadRequest(string.Format("Error creating or updating Profile for {0}, check ticker symbol.", tickerForProfile));

        //}


        


        [HttpGet]
        [Route("{tickerForProfile?}")]
        // e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            
            var updatedOrNewProfile = new Profile();
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

            // By default, we'll use the last 6 months for pricing history.
            var priceHistoryStartDate = CalculateStartDate(-180);

            using (var client = new HttpClient())
            {
                client.BaseAddress =
                    new Uri(BaseTiingoUrl + tickerForProfile); // https://api.tiingo.com/tiingo/daily/<ticker>
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TiingoAccountToken);

                HttpResponseMessage historicPriceDataResponse;
                HttpResponseMessage metaDataResponse;
                JArray jsonTickerPriceData;
                Task<string> responsePriceData;


                historicPriceDataResponse = await client.GetAsync(
                    client.BaseAddress + "/prices?startDate=" + priceHistoryStartDate + "&" + "token=" +
                    TiingoAccountToken);
                if (historicPriceDataResponse == null)
                    return BadRequest("Unable to update Profile price data for: " + tickerForProfile);

                responsePriceData = historicPriceDataResponse.Content.ReadAsStringAsync();
                jsonTickerPriceData = JArray.Parse(responsePriceData.Result);

                // Sort Newtonsoft JArray historical results on 'date', e.g., date info gathered.
                var orderedJsonTickerPriceData = new JArray(jsonTickerPriceData.OrderByDescending(obj => obj["date"]));

                var sequenceIndex = 0;
                var divCashGtZero = false;
                var metaDataInitialized = false;

                foreach (var objChild in orderedJsonTickerPriceData.Children<JObject>())
                {
                    if (existingProfile.Any())
                    {
                        // Profile update IF last updated > 72hrs ago.
                        if (Convert.ToDateTime(existingProfile.First().LastUpdate) > cutOffDateTimeForProfileUpdate)
                            return Ok(existingProfile.First());

                        // 11.17.2017 - Due to Tiingo API limitations, update just dividend rate.
                        foreach (var property in objChild.Properties())
                        {
                            if (property.Name != "divCash") continue;
                            var cashValue = decimal.Parse(property.Value.ToString());

                            if (cashValue <= 0) break;
                            existingProfile.First().DividendRate = decimal.Parse(property.Value.ToString());
                            existingProfile.First().DividendPayDate =
                                DateTime.Parse(objChild.Properties().ElementAt(0).Value.ToString());
                            existingProfile.First().Price =
                                decimal.Parse(objChild.Properties().ElementAt(1).Value.ToString());
                            existingProfile.First().DividendYield =
                                Utilities.CalculateDividendYield(existingProfile.First().DividendRate,
                                    existingProfile.First().Price);
                            existingProfile.First().LastUpdate = DateTime.Now;
                            updatedOrNewProfile = existingProfile.First();

                            // Update with any relevant saved info.
                            var updatedProfile2 = CheckPersistedProfile(updatedOrNewProfile, tickerForProfile);
                            updatedOrNewProfile = updatedProfile2.Result.Content;

                            return Ok(updatedOrNewProfile);
                        }

                        continue;
                    }

                    if (divCashGtZero)
                        break;


                    /* New Profile processing. Capture meta data. */
                    if (!metaDataInitialized)
                    {
                        metaDataResponse = await client.GetAsync(client.BaseAddress + "?token=" + TiingoAccountToken);
                        if (metaDataResponse == null)
                            return BadRequest("Unable to fetch Profile meta data for: " + tickerForProfile);

                        var responseMetaData = metaDataResponse.Content.ReadAsStringAsync();
                        var jsonTickerMetaData = JObject.Parse(await responseMetaData);

                        updatedOrNewProfile.TickerDescription = jsonTickerMetaData["name"].ToString().Trim();
                        updatedOrNewProfile.TickerSymbol = jsonTickerMetaData["ticker"].ToString().Trim();
                        metaDataResponse.Dispose();
                        metaDataInitialized = true;
                    }


                    // Capture most recent closing price & dividend rate (aka divCash); 
                    if (sequenceIndex == 0)
                    {
                        foreach (var property in objChild.Properties())
                        {
                            if (property.Name != "close") continue;
                            updatedOrNewProfile.Price = decimal.Parse(property.Value.ToString());
                            break;
                        }
                    }
                    else
                    {
                        foreach (var property in objChild.Properties())
                        {
                            if (property.Name != "divCash") continue;
                            var cashValue = decimal.Parse(property.Value.ToString());

                            if (cashValue <= 0) continue;
                            updatedOrNewProfile.DividendRate = decimal.Parse(property.Value.ToString());
                            updatedOrNewProfile.DividendYield =
                                Utilities.CalculateDividendYield(updatedOrNewProfile.DividendRate,
                                    updatedOrNewProfile.Price);
                            updatedOrNewProfile.DividendPayDate =
                                DateTime.Parse(objChild.Properties().ElementAt(0).Value.ToString());
                            divCashGtZero = true;
                            break;
                        }
                    }

                    sequenceIndex += 1;
                } // end foreach

                historicPriceDataResponse.Dispose();

            } // end using()


            // Apply any available persisted values as needed.
            var updatedProfile = CheckPersistedProfile(updatedOrNewProfile, tickerForProfile);
            updatedOrNewProfile = updatedProfile.Result.Content;

            updatedOrNewProfile.LastUpdate = DateTime.Now;
            return Ok(updatedOrNewProfile);

        } 




        [HttpPost]
        [Route("", Name = "CreateNewProfile")]
        public async Task<IHttpActionResult> CreateNewProfile([FromBody] ProfileVm submittedProfile)
        {
  
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for new Profile creation."
            });

            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == submittedProfile.TickerSymbol.Trim()));

            //TODO: Use configurable hours for: DateTime.Now.AddHours(-72)
            if (existingProfile.Any() && Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.Now.AddHours(-72))
                return ResponseMessage(new HttpResponseMessage {
                                                StatusCode = HttpStatusCode.NotModified, // Status:304
                                                ReasonPhrase = "No Profile created, existing data is less than 72 hours old."
                                     });

            var profileToCreate = MapVmToProfile(submittedProfile);
            var isCreated = await Task.FromResult(_repository.Create(profileToCreate));
            if (!isCreated)
                return BadRequest("Unable to create new Profile for :  " + submittedProfile.TickerSymbol.Trim());

            // TODO: This fx should be accessed via a HttpClient web call, so that we can fetch Request.RequestUrl for base server address.
            return Created("http://localhost/Pims.Web.Api/api/Profile/", profileToCreate);
           //return Created(submittedProfile.Url + "/" + profileToCreate.ProfileId, profileToCreate);
        }


        [HttpPut]
        [Route("~/api/Asset/{ticker}/Profile")]
        public async Task<IHttpActionResult> UpdateProfile([FromBody]ProfileVm editedProfile)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid/Incomplete data received for Profile update."
                });
            }

            var profileToUpdate = await GetProfileByTicker(editedProfile.TickerSymbol.Trim()) as OkNegotiatedContentResult<Profile>;
            if (profileToUpdate == null)
                return BadRequest("Unable to retreive Profile data for update.");

            var isUpdated = await Task.FromResult(_repository.Update(profileToUpdate.Content, profileToUpdate.Content.ProfileId));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update Profile for: {0}", editedProfile.TickerSymbol.Trim()));

            return Ok("Profile successfully updated for " + editedProfile.TickerSymbol.Trim());
        }



        private async Task<OkNegotiatedContentResult<Profile>> CheckPersistedProfile(Profile updatedOrNewProfile, string tickerForProfile)
        {
            if (updatedOrNewProfile.DividendRate == 0 || updatedOrNewProfile.DividendFreq == null || updatedOrNewProfile.DividendYield == null)
            {
                var actionResult = await Task.FromResult(GetPersistedProfileByTicker(tickerForProfile.Trim().ToUpper()));
                if (actionResult.Status != TaskStatus.Faulted)
                {
                    var persistedProfile = actionResult.Result as OkNegotiatedContentResult<Profile>;
                    if (persistedProfile != null)
                    {
                        updatedOrNewProfile.DividendFreq = updatedOrNewProfile.DividendFreq ?? persistedProfile.Content.DividendFreq;
                        updatedOrNewProfile.DividendRate = updatedOrNewProfile.DividendRate == 0
                            ? decimal.Parse(persistedProfile.Content.DividendFreq)
                            : updatedOrNewProfile.DividendRate;
                    }
                }
                else
                    actionResult.Dispose();
                
                
                updatedOrNewProfile.ProfileId = Guid.NewGuid();
                updatedOrNewProfile.AssetId = Guid.NewGuid();
                updatedOrNewProfile.EarningsPerShare = 0;
                updatedOrNewProfile.PE_Ratio = 0;
                updatedOrNewProfile.ExDividendDate = null;
            }

            return Ok(updatedOrNewProfile);

        }
        



        #region Helpers

            private static Profile MapVmToProfile(ProfileVm sourceData)
            {
                // TODO: Allow investor to change ex-dividend date & frequency.
                return new Profile
                       {
                           TickerSymbol = sourceData.TickerSymbol.ToUpper().Trim(),
                           TickerDescription = sourceData.TickerDescription.Trim(),
                           DividendRate = sourceData.DividendRate,
                           DividendYield = sourceData.DividendYield,
                           DividendFreq = sourceData.DividendFreq ?? "",
                           EarningsPerShare = sourceData.EarningsPerShare,
                           PE_Ratio = sourceData.PE_Ratio,
                           LastUpdate = DateTime.Now,
                           ExDividendDate = sourceData.ExDividendDate ?? new DateTime(2017, 1, 2),
                           DividendPayDate = sourceData.DividendPayDate,
                           Price = sourceData.Price,
                           Url = sourceData.Url == null ? "" : sourceData.Url.Trim()
                       };
            }
        
            private static string CalculateStartDate(int priorNumberOfDays)
            {
                if (priorNumberOfDays >= 0)
                    return "Invalid hours submitted.";

                var today = DateTime.Now;
                var priorDate = today.AddDays(priorNumberOfDays);
                return (priorDate.Year + "-" + priorDate.Month + "-" + priorDate.Day).Trim();
            }

            // Deferred
            //private static ProfileVm MapProfileToVm(Profile sourceData)
            //{
            //    return new ProfileVm
            //    {
            //        ProfileId = sourceData.ProfileId,
            //        TickerSymbol = sourceData.TickerSymbol,
            //        TickerDescription = sourceData.TickerDescription,
            //        DividendFreq = sourceData.DividendFreq,
            //        DividendRate = sourceData.DividendRate,
            //        DividendYield = sourceData.DividendYield,
            //        EarningsPerShare = sourceData.EarningsPerShare,
            //        PE_Ratio = sourceData.PE_Ratio,
            //        LastUpdate = sourceData.LastUpdate,
            //        ExDividendDate = sourceData.ExDividendDate,
            //        DividendPayDate = sourceData.DividendPayDate,
            //        Price = sourceData.Price
            //    };
            //}


        #endregion


    }

}
