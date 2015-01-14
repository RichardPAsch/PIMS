using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Profile")]
    public class ProfileController : ApiController
    {
        private static IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;

        public ProfileController(IPimsIdentityService identityService, IGenericRepository<Asset> repositoryAsset)
        {
            _identityService = identityService;
            _repositoryAsset = repositoryAsset;
            // inject dependency on YahooFinanceSvc ? how many actions need it?
        }


       
        [HttpGet]
        [Route("{tickerForProfile?}")]
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            Profile updatedOrNewProfile;
            var existingAsset = await Task.FromResult(_repositoryAsset.Retreive(a => a.Url.EndsWith(tickerForProfile))
                                                                      .AsQueryable()
                                                                      .FirstOrDefault(a => a.Profile.Url.Contains(tickerForProfile)));

            // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
            if (existingAsset != null)
            {
                // Update existing Profile only IF last updated > 24hrs ago.
                if (Convert.ToDateTime(existingAsset.Profile.LastUpdate) <= DateTime.UtcNow.AddHours(-24))
                {
                    updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, existingAsset.Profile));
                    if (updatedOrNewProfile != null)
                        return Ok(updatedOrNewProfile);

                    return BadRequest("Error updating Profile for ticker: " + tickerForProfile);  
                }
                return Ok(existingAsset.Profile);
            }

            updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, new Profile()));
            if (updatedOrNewProfile != null)
                return Ok(updatedOrNewProfile);

            return BadRequest(string.Format("Error creating Profile for {0}, check ticker symbol.", tickerForProfile));
            
        }
        
       
        [HttpDelete]
        [Route("{profileId}")]
        public async Task<IHttpActionResult> Delete(Guid profileId)
        {
            // No Profile can be deleted if referenced by any existing Asset belonging to another investor.
            var referencingAssets = await Task.FromResult(_repositoryAsset.RetreiveAll()
                                                                          .Where(a => a.Profile.ProfileId == profileId
                                                                                   && a.Investor.LastName != _identityService.CurrentUser).AsQueryable());

            if (referencingAssets.Any())
                return BadRequest(string.Format("Unable to delete; Profile {0} is in use.", profileId));

            var isDeleted = await Task<bool>.Factory.StartNew(
                       () => ((IGenericAggregateRepository)_repositoryAsset).AggregateDelete<Profile>(profileId));

            return isDeleted
                ? Ok(string.Format("Profile {0} successfully deleted.", profileId))
                : (IHttpActionResult)BadRequest("Error: unable to delete Profile: " + profileId);

        }


        [HttpPut]
        [HttpPatch]
        [Route("{tickerToUpdate}")]
        public async Task<IHttpActionResult> UpdateProfile([FromBody] Profile updatedProfile)
        {
           var isUpdated = await Task<bool>.Factory.StartNew(
                       () => ((IGenericAggregateRepository)_repositoryAsset).AggregateUpdate<Profile>(updatedProfile, string.Empty, updatedProfile.TickerSymbol.Trim()));

            return isUpdated
                ? Ok(updatedProfile)  // Newly persisted Profile update now returned to client.
                : (IHttpActionResult)BadRequest("Error: unable to update Profile: " + updatedProfile.TickerSymbol.Trim());

        }

       
    }

}
