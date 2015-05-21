using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Core.Security;
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
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            Profile updatedOrNewProfile;
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

            // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
            if (existingProfile != null)
            {
                // Update existing Profile only IF Profile last updated > 24hrs ago.
                if (Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.UtcNow.AddHours(-24))
                    return Ok(existingProfile.First());

                updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, existingProfile.First()));
                if (updatedOrNewProfile != null)
                    return Ok(updatedOrNewProfile);

                return BadRequest("Error updating Profile for ticker: " + tickerForProfile);
            }

            updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, new Profile()));
            if (updatedOrNewProfile != null)
                return Ok(updatedOrNewProfile);

            return BadRequest(string.Format("Error creating Profile for {0}, check ticker symbol.", tickerForProfile));

        }


        //TODO: Use configurable hours for: DateTime.UtcNow.AddHours(-24)
        [HttpPost]
        [Route("", Name = "CreateNewProfile")]
        public async Task<IHttpActionResult> CreateNewProfile([FromBody] Profile submittedProfile)
        {
  
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for new Profile creation."
            });

            // Double check data store that received Profile (from GET), is indeed current.
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == submittedProfile.TickerSymbol.Trim())); //&&
                                                                                  //Convert.ToDateTime(p.LastUpdate) > DateTime.UtcNow.AddHours(-24)) );


            if (existingProfile.Any() && Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.UtcNow.AddHours(-24))
                return ResponseMessage(new HttpResponseMessage {
                                                StatusCode = HttpStatusCode.NotModified, // Status:304
                                                ReasonPhrase = "No Profile created, existing data is currently up to date."
                                     });
            
            var isCreated = await Task.FromResult(_repository.Create(submittedProfile));
            if (!isCreated) return BadRequest("Unable to create new Profile for :  " + submittedProfile.TickerSymbol.Trim());

            var newLocation = Url.Link("CreateNewProfile", new { });
            return Created(newLocation, submittedProfile);
        }


    }

}
