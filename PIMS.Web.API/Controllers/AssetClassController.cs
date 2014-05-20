using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    public class AssetClassController : ApiController
    {

        private static IGenericRepository<AssetClass> _repository;
        public AssetClassController(IGenericRepository<AssetClass> repository )
        {
            _repository = repository;
        }

       
        public IQueryable<AssetClass> Get()
        {
            var classifications = _repository.RetreiveAll().OrderBy(c => c.Code);

            return classifications;
        }


        public HttpResponseMessage Get(HttpRequestMessage req, string assetClassCode)
        {
           var classification = _repository.Retreive(assetClassCode);
           return classification == null 
               ? req.CreateResponse(HttpStatusCode.NotFound) 
               : req.CreateResponse(HttpStatusCode.OK, classification);
        }


        public HttpResponseMessage Get(HttpRequestMessage req, Guid? id)
        {
            var classification = new AssetClass();
            if (id != null)
            {
                classification = _repository.RetreiveById((Guid) id);


                if (classification == null) {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                return req.CreateResponse(HttpStatusCode.OK, classification);
            }
            return req.CreateResponse(HttpStatusCode.OK, classification);
        }


        // Guid parameter name (id) must match RouteTemplate name in WebApiConfig.
        public HttpResponseMessage Delete(HttpRequestMessage req, Guid id )
        {
            return !_repository.Delete(id) 
                ? req.CreateErrorResponse(HttpStatusCode.NotFound, "Asset class could not be removed, or was not found.") 
                : req.CreateResponse(HttpStatusCode.OK);
        }


        public HttpResponseMessage Put([FromBody] AssetClass updatedClassification, HttpRequestMessage req) {

            // No UOW repository or interface is necessary, as "Classifcation" involves no
            // logical transactions or object graphs, when used by itself.
            try
            {
                return req.CreateResponse(_repository.Update(updatedClassification, updatedClassification.KeyId) 
                    ? HttpStatusCode.OK 
                    : HttpStatusCode.NotFound, updatedClassification);
            }
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to update AssetClass: " 
                    + updatedClassification.Code + " due to: " + ex.Message);
            }
        }


        public HttpResponseMessage Post([FromBody] AssetClass newClassification, HttpRequestMessage requestMessage)
        {
            // No UOW repository or interface is necessary, as "AssetClass" involves no
            // logical transactions or object graphs, when used by itself.
            var response = requestMessage.CreateResponse(HttpStatusCode.Created, newClassification);
            if (_repository.Create(newClassification))
            {
                try
                {
                    // TODO: Why do we get a relative URL here (commented code for uri); use of "newClassification.Code" - correct?
                    // Route name must match existing route in WebApiConfig.
                    //var uri = Url.Link("AssetClassRoute", new {controller = "AssetClass", Code = newClassification.Code});
                    var uri = requestMessage.RequestUri.AbsoluteUri + "/" + newClassification.Code;
                    response.Headers.Location = new Uri(uri);
                    //if (uri != null) response.Headers.Location = new Uri(uri);
                }
                catch (Exception ex)
                {
                    response = requestMessage.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Unable to add new Asset class, due to: " + ex.Message);
                }
            }
            else
            {
                response = requestMessage.CreateErrorResponse(HttpStatusCode.Conflict, "Duplicate entry found for " + newClassification.Code);
            }

            return response;
        }


       

 
    }
}
