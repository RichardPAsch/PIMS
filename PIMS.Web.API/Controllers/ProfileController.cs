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
        //private static IGenericRepository<Profile> _repository;
        private static IGenericRepository<Asset> _repositoryAsset;

        public ProfileController(IGenericRepository<Asset> repositoryAsset)
        {
            //_repository = repository;
            _repositoryAsset = repositoryAsset;
        }


        // Get Profile for a given Asset.
        public HttpResponseMessage Get(HttpRequestMessage req, string ticker)
        {
            var response = new HttpResponseMessage();
            try
            {
                var selectedAsset = _repositoryAsset.Retreive(ticker);
                var assetProfile = selectedAsset.Profile;

                // Ascertain we have the right Profile for the right Asset.
                if (assetProfile.TickerSymbol == selectedAsset.AssetClass.Code)
                    response = req.CreateResponse(HttpStatusCode.OK, assetProfile);
            }
            catch (Exception ex)
            {

                response = req.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for :" + ticker + " due to " + ex.Message);
            }

            return response;
        }


        // Get Profile for a given Asset.
        public HttpResponseMessage Get(HttpRequestMessage req, Guid assetId) {
            var response = new HttpResponseMessage();
            try {
                var selectedAsset = _repositoryAsset.RetreiveById(assetId);
                var assetProfile = selectedAsset.Profile;

                // Ascertain we have the right Profile for the right Asset.
                if (assetProfile.TickerSymbol == selectedAsset.AssetClass.Code)
                    response = req.CreateResponse(HttpStatusCode.OK, assetProfile);
            }
            catch (Exception ex) {

                response = req.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to get Profile data for AssetId :" + assetId + " due to " + ex.Message);
            }

            return response;
        }




        // TODO: to be implemented
        public HttpResponseMessage Post([FromBody] Profile newProfile, HttpRequestMessage requestMessage)
        {
            //var response = requestMessage.CreateResponse(HttpStatusCode.Created, newProfile);
            //if (_repository.Create(newProfile)) {
            //    try {
            //        // Route name must match existing route in WebApiConfig.
            //        var uri = Url.Link("ProfileRoute", new { controller = "Profile", ticker = newProfile.TickerSymbol });
            //        if (uri != null) response.Headers.Location = new Uri(uri);
            //    }
            //    catch (Exception ex) {
            //        response = requestMessage.CreateErrorResponse(HttpStatusCode.BadRequest,
            //                                    "New Profile not added, due to: " + ex.Message);
            //    }
            //} else {
            //    response = requestMessage.CreateErrorResponse(HttpStatusCode.Conflict, "Duplicate entry found for " + newProfile.TickerSymbol);
            //}

            //return response;
            return null;

        }


    }
}
