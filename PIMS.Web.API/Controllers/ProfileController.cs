using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using PIMS.Core.Models;
using PIMS.Data;
using PIMS.Data.Repositories;
using PIMS.Core.Models.ViewModels;


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
        

        public ProfileController(IGenericRepository<Profile> repository)
        {
            _repository = repository;
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
        [Route("{tickerForProfile?}")]
        // e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            Profile updatedOrNewProfile;
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

            // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
            if (existingProfile.Any() )
            {
                // Update existing Profile only IF Profile last updated > 24hrs ago.
                if (Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.UtcNow.AddHours(-24))
                    return Ok(existingProfile.First());

                updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), existingProfile.First()));
                if (updatedOrNewProfile != null)
                    return Ok(MapProfileToVm(updatedOrNewProfile));

                return BadRequest("Error updating Profile for ticker: " + tickerForProfile);
            }

            updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), new Profile()));
            if (updatedOrNewProfile != null)
                return Ok(updatedOrNewProfile);

            return BadRequest(string.Format("Error creating Profile for {0}, check ticker symbol.", tickerForProfile));

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
            

            // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=<tickers>&f=sodyrr1
            //if (existingProfile.Any()) {
            //    // Update existing Profile only IF Profile last updated > 24hrs ago.
            //    if (Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.UtcNow.AddHours(-24))
            //        return Ok(existingProfile.First());

            //    updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), existingProfile.First()));
            //    if (updatedOrNewProfile != null)
            //        return Ok(MapProfileToVm(updatedOrNewProfile));

            //    return BadRequest("Error updating Profile for ticker: " + tickerForProfile);
            //}

            var initializedProfiles = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfiles(tickers));
            if (initializedProfiles != null)
                return Ok(initializedProfiles);

            return BadRequest(string.Format("Error obtaining Profile data due to connectivity, OR invalid submitted ticker symbols. Check ticker accuracy."));
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

            //TODO: Use configurable hours for: DateTime.Now.AddHours(-24)
            if (existingProfile.Any() && Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.Now.AddHours(-24))
                return ResponseMessage(new HttpResponseMessage {
                                                StatusCode = HttpStatusCode.NotModified, // Status:304
                                                ReasonPhrase = "No Profile created, existing data is less than 24 hours old."
                                     });

            var profileToCreate = MapVmToProfile(submittedProfile);
            var isCreated = await Task.FromResult(_repository.Create(profileToCreate));
            if (!isCreated) return BadRequest("Unable to create new Profile for :  " + submittedProfile.TickerSymbol.Trim());

            //TODO: Should use RouteData for determining newLocation Url. 7-8-15: Reeval, RouteData avail via RPC?
           // return Created("http://localhost/Pims.Web.Api/api/Asset/", profileToCreate);
            return Created(submittedProfile.Url + "/" + profileToCreate.ProfileId, profileToCreate);
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




        #region Helpers

            private static Profile MapVmToProfile(ProfileVm sourceData)
            {
                return new Profile
                       {
                           TickerSymbol = sourceData.TickerSymbol.ToUpper().Trim(),
                           TickerDescription = sourceData.TickerDescription.Trim(),
                           DividendRate = sourceData.DividendRate,
                           DividendYield = sourceData.DividendYield,
                           DividendFreq = sourceData.DividendFreq,
                           EarningsPerShare = sourceData.EarningsPerShare,
                           PE_Ratio = sourceData.PE_Ratio,
                           LastUpdate = DateTime.Now,
                           ExDividendDate = sourceData.ExDividendDate,
                           DividendPayDate = sourceData.DividendPayDate,
                           Price = sourceData.Price,
                           Url = sourceData.Url.Trim()
                       };
            }

            private static ProfileVm MapProfileToVm(Profile sourceData)
            {
                return new ProfileVm
                {
                    ProfileId = sourceData.ProfileId,
                    TickerSymbol = sourceData.TickerSymbol,
                    TickerDescription = sourceData.TickerDescription,
                    DividendFreq = sourceData.DividendFreq,
                    DividendRate = sourceData.DividendRate,
                    DividendYield = sourceData.DividendYield,
                    EarningsPerShare = sourceData.EarningsPerShare,
                    PE_Ratio = sourceData.PE_Ratio,
                    LastUpdate = sourceData.LastUpdate,
                    ExDividendDate = sourceData.ExDividendDate,
                    DividendPayDate = sourceData.DividendPayDate,
                    Price = sourceData.Price
                };
            }

       

        #endregion


    }

}
