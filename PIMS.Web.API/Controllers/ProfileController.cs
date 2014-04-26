using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    /* Notes:
       1. Support only for /api/Asset/<ticker>/Profile       - GET
       2. Support only for /api/Asset/<ticker>/Profile/<id>  - PUT
       3. Creation delegated to ProfileRepository.Save() for /api/Asset/<ticker>/Profile - POST - are we sure ??
    */


    public class ProfileController : ApiController
    {
        private static IGenericRepository<Profile> _repository;

        public ProfileController(IGenericRepository<Profile> repositoryProfile)
        {
            _repository = repositoryProfile;
        }



        public HttpResponseMessage Get(HttpRequestMessage req, string ticker)
        {
            var response = new HttpResponseMessage();
            try
            {
                var selectedProfile = _repository.Retreive(ticker);

                // Ascertain we have the right Profile for the right Asset.
                if (selectedProfile.TickerSymbol == ticker)
                    response = req.CreateResponse(HttpStatusCode.OK, selectedProfile);
            }
            catch (Exception ex)
            {
                response = req.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for :" + ticker + " due to " + ex.Message);
            }

            return response;
        }


        public HttpResponseMessage Get(HttpRequestMessage req, Guid profileId) {
            var response = new HttpResponseMessage();
            try {
                var selectedProfile = _repository.RetreiveById(profileId);

                // Ascertain we have the right Profile for the right Asset.
                if (selectedProfile.ProfileId == profileId)
                    response = req.CreateResponse(HttpStatusCode.OK, selectedProfile);
            }
            catch (Exception ex) {

                response = req.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for AssetId :" + profileId + " due to " + ex.Message);
            }

            return response;
        }
        

        public HttpResponseMessage Post([FromBody] Profile newProfile, HttpRequestMessage requestMessage)
        {
            // Adding a Profile via an Asset POST/PUT from:
            // 1. Profile data via Yahoo.Finance, OR
            // 2. existing backend Profile record
            // 3. URL - /api/Asset/<ticker>/Profile 


            var response = requestMessage.CreateResponse(HttpStatusCode.Created, newProfile);
            if (_repository.Create(newProfile)) {
                try
                {
                    // Route name must match existing route in WebApiConfig.
                    // TODO: Try to get this to work using Url.Link(). Work around needs to be tested via Fiddler.
                    //var uri = Url.Link("ProfileRoute", new { controller = "Profile"});
                    var uri = requestMessage.RequestUri.AbsoluteUri;
                    response.Headers.Location = new Uri(uri);
                }
                catch (Exception ex) {
                    response = requestMessage.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "New Profile not added, due to: " + ex.Message);
                }
            } else {
                response = requestMessage.CreateErrorResponse(HttpStatusCode.Conflict, "Duplicate entry found for " + newProfile.TickerSymbol);
            }

            return response;

        }


        public HttpResponseMessage Put([FromBody] Profile revisedProfile, HttpRequestMessage req) {

            if (_repository.Update(revisedProfile)) {
                return req.CreateResponse(HttpStatusCode.OK, revisedProfile);
            }

            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to update Profile for: " + revisedProfile.TickerSymbol);
        }


        public HttpResponseMessage Delete(Guid id, HttpRequestMessage req) {

            if (!_repository.Delete(id)) {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, "Profile could not be removed, or was not found.");
            }

            return req.CreateResponse(HttpStatusCode.OK);

        }
    }
}
