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
using PIMS.Core.Security;
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
        private readonly IPimsIdentityService _identityService;
        private const string BaseTiingoUrl = "https://api.tiingo.com/tiingo/daily/";
        private const string TiingoAccountToken = "95cff258ce493ec51fd10798b3e7f0657ee37740";
        private DateTime cutOffDateTimeForProfileUpdate = DateTime.Now.AddHours(-72);


        public ProfileController(IGenericRepository<Profile> repository, IPimsIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }



        [HttpGet]
        [Route("persisted/{tickerSymbol}")]
        public async Task<IHttpActionResult> GetPersistedProfileByTicker(string tickerSymbol)
        {
            var savedProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol == tickerSymbol).AsQueryable());
   
            if(!savedProfile.Any())
                return BadRequest("No saved Profile found, or unable to retreive data for ticker: " + tickerSymbol);

            return Ok(savedProfile.First());
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
        // t = ticker; used primarily in context of revenue 'Projections' forecasting.
        public async Task<IHttpActionResult> GetProfiles(string t1, string t2 = "", string t3 = "", string t4 = "", string t5 = "") {

            if (string.IsNullOrEmpty(t1))
                return BadRequest(string.Format("Error fetching Profile, minimum of 1 ticker symbol required."));

            // Track (N)ot (A)vailable Tiingo API Profile data.
            var tickerNaCountGtZero = false;

            var tickersTemp = string.Empty;
            tickersTemp += t1;
            if (!string.IsNullOrEmpty(t2)) tickersTemp += "," + t2;
            if (!string.IsNullOrEmpty(t3)) tickersTemp += "," + t3;
            if (!string.IsNullOrEmpty(t4)) tickersTemp += "," + t4;
            if (!string.IsNullOrEmpty(t5)) tickersTemp += "," + t5;

            var tickers = tickersTemp.Split(',');

            ProfileProjectionVm projectionVm = null;
            var profileProjections = new List<ProfileProjectionVm>();
            foreach (var ticker in tickers)
            {
                try
                {
                   var webTiingoResult = await GetProfileByTicker(ticker) as OkNegotiatedContentResult<Profile>;
                   if (webTiingoResult != null)
                    {
                        var profileContent = webTiingoResult.Content;
                        projectionVm = new ProfileProjectionVm
                                           {
                                               Ticker = profileContent.TickerSymbol.ToUpper().Trim(),
                                               Capital = 0,
                                               ProjectedRevenue = 0,
                                               Price = profileContent.Price,
                                               DivRate = profileContent.DividendRate > 0 ? profileContent.DividendRate : 0
                                           };
                    }
                    else
                    {
                        // Capture exception ticker (no Profile data available via Tiingo) for later database check(s).
                        tickerNaCountGtZero = true;
                        projectionVm = new ProfileProjectionVm
                                       {
                                           Ticker = ticker.Trim().ToUpper() + "-NA",
                                           Capital = 0,
                                           ProjectedRevenue = 0,
                                           Price = 0,
                                           DivRate = 0
                                       };
                    }
                    profileProjections.Add(projectionVm);
                }
                catch (Exception e)
                {
                    return BadRequest("Error obtaining Profile data via web for \n\'projections\' calculations. Check ticker accuracy.");
                }
            }


            // If necessary, check against any saved Profile(s) info, before returning projection results.
            if (tickerNaCountGtZero)
            {
                foreach (var projection in profileProjections)
                {
                    if (projection.Ticker.IndexOf("NA", StringComparison.Ordinal) < 0) continue;
                    var tempProfile = new Profile
                                      {
                                          TickerSymbol = projection.Ticker.Substring(0,projection.Ticker.IndexOf("-", StringComparison.Ordinal)),
                                          DividendRate = 0,
                                          DividendFreq = null
                                      };

                    var persistedProfile = CheckPersistedProfile(tempProfile, tempProfile.TickerSymbol).Result.Content;
                        
                    // Updated projection data as needed.
                    if (string.IsNullOrEmpty(persistedProfile.TickerSymbol))
                        projection.Ticker = persistedProfile.TickerSymbol.Trim().ToUpper() + "-NA";

                    if (persistedProfile.DividendRate <= 0 || persistedProfile.Price <= 0) continue;
                    projection.DivRate = persistedProfile.DividendRate;
                    projection.Price = persistedProfile.Price;


                }
            }


            return profileProjections.Count > 0 ?  Ok(profileProjections.ToArray()) : null;

        }
        

        /*  ** NOTE **
            11.9.2017 -  GetProfileByTicker() & GetProfiles() are now OBSOLETE due to the shutdown of Yahoo Finance API.
                         New ticker profile data now supplied via Tiingo.com API.
            11.10.2017 - Seperate API calls will be necessary for gathering meta & price data, as mandated by Tiingo.
        */

     
        [HttpGet]
        [Route("{tickerForProfile?}")]
        // e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {

            // Allow for Fiddler debugging
            var currentInvestor = _identityService.CurrentUser;
            if (currentInvestor == null)
                currentInvestor = "rpasch@rpclassics.net";  // temp login for Fiddler TESTING
            //currentInvestor = "maryblow@yahoo.com";

            var existingSavedProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());
            var updatedOrNewProfile = new Profile();
            var foundTiingoData = false;
            
            // By default, we'll use the last 6 months of Tiingo pricing historical data.
            var priceHistoryStartDate = CalculateStartDate(-180);

            // Bypass Tiingo if dealing with a custom Profile.
            if (existingSavedProfile.Any() && existingSavedProfile.First().CreatedBy != null)
            {
                if(existingSavedProfile.First().CreatedBy.Trim() == currentInvestor.Trim())
                    return Ok(existingSavedProfile.First());
            }
                

            var divCashGtZero = false; // 'divCash' (per Tiingo API), aka dividend rate.
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseTiingoUrl + tickerForProfile); // https://api.tiingo.com/tiingo/daily/<ticker>
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TiingoAccountToken);

                HttpResponseMessage historicPriceDataResponse;
                HttpResponseMessage metaDataResponse;
                JArray jsonTickerPriceData;
                Task<string> responsePriceData;

               
                // If possible, try to obtain updated Profile info via Tiingo API.
                historicPriceDataResponse = await client.GetAsync(client.BaseAddress + "/prices?startDate=" + priceHistoryStartDate + "&" + "token=" + TiingoAccountToken);
                if (historicPriceDataResponse == null || !historicPriceDataResponse.IsSuccessStatusCode)
                    return BadRequest("Unable to fetch, or no Profile history found via web for: " + tickerForProfile);

                responsePriceData = historicPriceDataResponse.Content.ReadAsStringAsync();
                jsonTickerPriceData = JArray.Parse(responsePriceData.Result);

                // Sort Newtonsoft JArray historical results on 'date', e.g., date info gathered.
                var orderedJsonTickerPriceData = new JArray(jsonTickerPriceData.OrderByDescending(obj => obj["date"]));
                
                var sequenceIndex = 0;
                var metaDataInitialized = false;

                // Check Tiingo historical results for this ticker.
                if (orderedJsonTickerPriceData.Count > 0)
                {
                    foundTiingoData = true;
                    foreach (var objChild in orderedJsonTickerPriceData.Children<JObject>())
                    {
                        if (existingSavedProfile.Any())
                        {
                            // If applicable, update existing Profile IF last update was  > 72hrs ago.
                            if (Convert.ToDateTime(existingSavedProfile.First().LastUpdate) > cutOffDateTimeForProfileUpdate)
                                return Ok(existingSavedProfile.First());

                            // 11.17.2017 - Due to Tiingo API limitations, update just dividend rate.
                            foreach (var property in objChild.Properties())
                            {
                                if (property.Name != "divCash") continue;
                                var cashValue = decimal.Parse(property.Value.ToString());

                                if (cashValue <= 0) break;
                                divCashGtZero = true;
                                existingSavedProfile.First().DividendRate = decimal.Parse(property.Value.ToString());
                                existingSavedProfile.First().DividendPayDate = DateTime.Parse(objChild.Properties().ElementAt(0).Value.ToString());
                                existingSavedProfile.First().Price = decimal.Parse(objChild.Properties().ElementAt(1).Value.ToString());
                                existingSavedProfile.First().DividendYield = Utilities.CalculateDividendYield(existingSavedProfile.First().DividendRate, existingSavedProfile.First().Price);
                                existingSavedProfile.First().LastUpdate = DateTime.Now;
                                updatedOrNewProfile = existingSavedProfile.First();

                                // Update with any relevant saved info.
                                var updatedProfile2 = CheckPersistedProfile(updatedOrNewProfile, tickerForProfile);
                                updatedOrNewProfile = updatedProfile2.Result.Content;
                                   
                                return Ok(updatedOrNewProfile);
                            }

                            continue;
                        }

                        if (divCashGtZero)
                            break;


                        /* New Profile processing. Capture meta data for ticker info. */
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


                        // Capture most recent closing price & where applicable, the dividend rate (aka divCash); 
                        if (sequenceIndex == 0)
                        {
                            foreach (var property in objChild.Properties())
                            {
                                if (property.Name == "divCash" && decimal.Parse(property.Value.ToString()) > 0)
                                {
                                    updatedOrNewProfile.DividendRate = decimal.Parse(property.Value.ToString());
                                    divCashGtZero = true;
                                }

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
                                updatedOrNewProfile.DividendYield = Utilities.CalculateDividendYield(updatedOrNewProfile.DividendRate, updatedOrNewProfile.Price);
                                updatedOrNewProfile.DividendPayDate = DateTime.Parse(objChild.Properties().ElementAt(0).Value.ToString());
                                divCashGtZero = true;
                                break;
                            }
                        }

                        sequenceIndex += 1;
                    } // end foreach 

               } // enf if

                historicPriceDataResponse.Dispose();

            } // end using()


            if(foundTiingoData && divCashGtZero)
                return Ok(updatedOrNewProfile);

            if(!foundTiingoData && existingSavedProfile.Any())
                return Ok(existingSavedProfile.First());

            return null;
        } 
        

        
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateNewProfile([FromBody] ProfileVm submittedProfile)
        {
            var currentInvestor = string.Empty;
            if(_identityService != null && _identityService.CurrentUser != null)
                currentInvestor = _identityService.CurrentUser;
            //else if(loggedInvestor != "unkown")
            //    currentInvestor = loggedInvestor;

            
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

            // Leverage 'CreatedBy' attribute; initialized temporarily to 'XLXS' when Profile is being created as part
            // of the Asset creation process, e.g., resulting from new Position data introduced via XLSX spreadsheet.
            if (submittedProfile.CreatedBy.ToUpper().Trim() == "XLSX")
                submittedProfile.CreatedBy = string.Empty;
            else
            {
                if(string.IsNullOrEmpty(currentInvestor))
                    return BadRequest("Please log in, or register, to create a custom Profile.");
                
                submittedProfile.CreatedBy = currentInvestor;
            }
           
            var profileToCreate = MapVmToProfile(submittedProfile);

            var isCreated = await Task.FromResult(_repository.Create(profileToCreate));
            if (!isCreated)
                return BadRequest("Unable to create new Profile for :  " + submittedProfile.TickerSymbol.Trim());

            // TODO: This fx should be accessed via a HttpClient web call, so that we can fetch Request.RequestUrl for base server address.
            return Created("http://localhost/Pims.Web.Api/api/Profile/", profileToCreate);
        }



        [HttpPost]
        [Route("~/api/Asset/{ticker}/Profile")]
        public async Task<IHttpActionResult> UpdateProfile([FromBody]ProfileVm editedProfile)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid/Incomplete data received for Profile update."
                });
            }

            var profileToUpdate = MapVmToProfile( editedProfile);
            var isUpdated = await Task.FromResult(_repository.Update(profileToUpdate, profileToUpdate.ProfileId));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update Profile for: {0}", editedProfile.TickerSymbol.Trim()));

            return Ok("Profile successfully updated for " + editedProfile.TickerSymbol.Trim());
        }



        private async Task<OkNegotiatedContentResult<Profile>> CheckPersistedProfile(Profile updatedOrNewProfile, string tickerForProfile)
        {
            if (updatedOrNewProfile.DividendRate == 0 || updatedOrNewProfile.DividendFreq == null || updatedOrNewProfile.DividendYield == null)
            {
                var persistedResult = await Task.FromResult(GetPersistedProfileByTicker(tickerForProfile.Trim().ToUpper()));
                if (persistedResult.Status != TaskStatus.Faulted)
                {
                    var persistedProfile = persistedResult.Result as OkNegotiatedContentResult<Profile>;
                    if (persistedProfile != null)
                    {
                        updatedOrNewProfile.DividendFreq = persistedProfile.Content.DividendFreq ?? updatedOrNewProfile.DividendFreq;
                        updatedOrNewProfile.DividendRate = persistedProfile.Content.DividendRate == 0 ? 0 : persistedProfile.Content.DividendRate;
                        updatedOrNewProfile.Price = persistedProfile.Content.Price > 0 ? persistedProfile.Content.Price : 0;
                    }
                }
               
                persistedResult.Dispose();
                
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
                           ProfileId = sourceData.ProfileId,
                           TickerSymbol = sourceData.TickerSymbol.ToUpper().Trim(),
                           TickerDescription = sourceData.TickerDescription.Trim(),
                           CreatedBy = string.IsNullOrEmpty(sourceData.CreatedBy) ? "" : sourceData.CreatedBy,
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

        #endregion


    }

}
