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

        public ProfileController(IGenericRepository<Profile> repository )
        {
            _repository = repository;
        }


        public HttpResponseMessage Get(HttpRequestMessage req, string ticker) {

            var assetProfile = _repository.Retreive(ticker);

            return assetProfile == null ? req.CreateResponse(HttpStatusCode.NotFound) : req.CreateResponse(HttpStatusCode.OK, assetProfile);
        }


        public HttpResponseMessage Post([FromBody] Profile newProfile, HttpRequestMessage requestMessage)
        {
            var response = requestMessage.CreateResponse(HttpStatusCode.Created, newProfile);
            if (_repository.Create(newProfile)) {
                try {
                    // Route name must match existing route in WebApiConfig.
                    var uri = Url.Link("ProfileRoute", new { controller = "Profile", ticker = newProfile.TickerSymbol });
                    if (uri != null) response.Headers.Location = new Uri(uri);
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


    }
}
