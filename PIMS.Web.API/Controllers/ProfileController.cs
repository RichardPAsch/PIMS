using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    
    public class ProfileController : ApiController
    {
        private static IGenericRepository<Profile> _repository;
        public ProfileController(IGenericRepository<Profile> repositoryProfile)
        {
            _repository = repositoryProfile;
        }



        public HttpResponseMessage Get(HttpRequestMessage req, string ticker) {
            HttpResponseMessage response;
            try {
                var selectedProfile = _repository.Retreive(ticker);

                // Ascertain we have the right Profile for the right Asset.
                if (selectedProfile.TickerSymbol == ticker)
                    response = req.CreateResponse(HttpStatusCode.OK, selectedProfile);
                else {
                    // Most likely a bad ticker symbol.
                    throw new Exception();
                }
            }
            catch (Exception ex) {
                response = req.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for :" + ticker + " due to " + ex.Message);
            }

            return response;
        }
        
        public HttpResponseMessage Get(HttpRequestMessage request, Guid profileId = new Guid()) {
            // Default Guid param intialization required to avoid HTTP 500 error.
            var response = new HttpResponseMessage();
            try {
                var selectedProfile = _repository.RetreiveById(profileId);

                // Ascertain we have the right Profile for the right Asset.
                if (selectedProfile.ProfileId == profileId)
                    response = request.CreateResponse(HttpStatusCode.OK, selectedProfile);
            }
            catch (Exception ex) {

                response = request.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for AssetId :" + profileId + " due to " + ex.Message);
            }

            return response;
        }
        
        public HttpResponseMessage Post([FromBody] Profile newProfile, HttpRequestMessage requestMessage)
        {
            var response = requestMessage.CreateResponse(HttpStatusCode.Created, newProfile);
            if (_repository.Create(newProfile)) {
                try
                {
                    // Route name must match existing route in WebApiConfig.
                    // TODO: Try to get this to work using Url.Link() with relative path--using IIS server. Work around needs to be tested via Fiddler.
                    var uri = requestMessage.RequestUri.AbsoluteUri;
                    response.Headers.Location = new Uri(uri);
                }
                catch (Exception ex) {
                    response = requestMessage.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "New Profile not added, due to: " + ex.Message);
                }
            } else {
                response = requestMessage.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Unable to save Profile, or duplicate entry found for " + newProfile.TickerSymbol);
            }

            return response;
        }


        public HttpResponseMessage Put([FromBody] Profile updatedProfile, HttpRequestMessage req)
        {
            // For now, we'll update the Profile, regardless of how old our existing record is.
            HttpResponseMessage response;
            try
            {
                // The received updated Profile will not have a modified Id.
                var existingProfile = _repository.RetreiveById(updatedProfile.ProfileId);
                if (existingProfile == null) return req.CreateErrorResponse(HttpStatusCode.NotFound, "No Profile found.");

                existingProfile = updatedProfile;
                var isUpdated = _repository.Update(existingProfile, existingProfile.ProfileId);
                response = isUpdated ? req.CreateResponse(HttpStatusCode.OK) 
                                     : req.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to update Profile : " + updatedProfile.TickerSymbol);
            }
            catch (Exception ex)
            {
                response = req.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to update Profile due to: " + ex.Message);
            }

            return response;
        }


        public HttpResponseMessage Delete(HttpRequestMessage req, Guid profileId = new Guid())
        {
            //  TODO: Requires ADMIN access.
            return !_repository.Delete(profileId) 
                ? req.CreateErrorResponse(HttpStatusCode.NotFound, "Profile could not be removed, or was not found.") 
                : req.CreateResponse(HttpStatusCode.OK);
        }
    }

}
