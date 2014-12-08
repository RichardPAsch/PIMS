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
        private static IGenericRepository<Profile> _repository;
        private static IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;

        public ProfileController(IGenericRepository<Profile> repositoryProfile, IPimsIdentityService identityService, IGenericRepository<Asset> repositoryAsset)
        {
            _repository = repositoryProfile;
            _identityService = identityService;
            _repositoryAsset = repositoryAsset;
            // inject dependency on YahooFinanceSvc ? how many actions need it?
        }

       
        [HttpGet]
        [Route("{tickerForProfile?}")]
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            Profile processedProfile;
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.ToUpper().Trim() == tickerForProfile.Trim().ToUpper()).AsQueryable());
            
            // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
            if (existingProfile.Any())
            {
                processedProfile = YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, existingProfile.First());
                if (processedProfile != null)
                    return Ok(processedProfile);

                return BadRequest("Error updating Profile for ticker: " + tickerForProfile);
            }
              
            processedProfile = YahooFinanceSvc.ProcessYahooProfile(tickerForProfile, new Profile());
            if (processedProfile != null)
                return Ok(processedProfile);

            return BadRequest( string.Format("Error creating Profile for {0}, check ticker symbol.)", tickerForProfile));
           
        }
        

       
        [HttpDelete]
        [Route("{profileId}")]
        public async Task<IHttpActionResult> Delete(Guid profileId)
        {
            // No Profile can be deleted if referenced by any existing Asset not belonginh to the current investor.
            var currentInvestor = _identityService.CurrentUser;
            var referencedAssets = await Task.FromResult(_repositoryAsset.RetreiveAll()
                                             .Where(a => a.Profile.ProfileId == profileId && a.Investor.LastName != currentInvestor).AsQueryable());


            if (referencedAssets.Any())
                return BadRequest(string.Format("Unable to delete; Profile {0} is in use.", profileId));


            var isDeleted = _repository.Delete(profileId);
            
            if (isDeleted)
                return Ok(string.Format("Profile {0} successfully deleted", profileId));

            return BadRequest("Unable to delete Profile: " + profileId);

        }


       
    }

}
