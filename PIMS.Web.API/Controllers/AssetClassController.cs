﻿using System;
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


        public HttpResponseMessage Get(HttpRequestMessage req, string code)
        {
           var classification = _repository.Retreive(code);


            if (classification == null) {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            return req.CreateResponse(HttpStatusCode.OK, classification);
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
        public HttpResponseMessage Delete(Guid id, HttpRequestMessage req) {

            if (!_repository.Delete(id)) {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, "Asset class could not be removed, or was not found.");
            }
            
            return req.CreateResponse(HttpStatusCode.OK);

        }


        public HttpResponseMessage Put([FromBody] AssetClass updatedClassification, HttpRequestMessage req) {

            // No UOW repository or interface is necessary, as "Classifcation" involves no
            // logical transactions or object graphs, when used by itself.

            if (_repository.Update(updatedClassification)) {
                return req.CreateResponse(HttpStatusCode.OK, updatedClassification);
            }

            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to update AssetClass: " + updatedClassification.Code);
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
                    // Route name must match existing route in WebApiConfig.
                    var uri = Url.Link("AssetClassRoute", new {controller = "AssetClass", Code = newClassification.Code});
                    if (uri != null) response.Headers.Location = new Uri(uri);
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
