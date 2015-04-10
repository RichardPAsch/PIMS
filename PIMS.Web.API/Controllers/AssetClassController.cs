using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentNHibernate.Conventions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/AssetClass")]
    public class AssetClassController : ApiController
    {

        private static IGenericRepository<AssetClass> _repository;

        public AssetClassController(IGenericRepository<AssetClass> repository )
        {
            _repository = repository;
        }



        
        [HttpGet]
        [Route("")]
        public async Task<IQueryable<AssetClass>> GetAll()
        {
            return await Task<IQueryable<AssetClass>>.Factory.StartNew(() => _repository.RetreiveAll().OrderBy(ac => ac.Code));
        }


        [HttpGet] 
        [Route("{assetClassId:guid}")]
        public async Task<IHttpActionResult> GetByClassificationId( Guid assetClassId)
        {
            var matchingAssetClass = await Task.FromResult(_repository.RetreiveById(assetClassId));
            return matchingAssetClass != null
                ? (IHttpActionResult) Ok(matchingAssetClass)
                : BadRequest("No AssetClassification found matching " + assetClassId);
        }


        [HttpGet]
        [Route("{code}")]
        public async Task<IHttpActionResult> GetByClassification(string code)
        {
            var matchingAssetClass = await Task.FromResult(_repository.Retreive(ac => ac.Code.Trim() == code.Trim())
                                                                      .AsQueryable());

            if (matchingAssetClass.Any())
                return Ok(matchingAssetClass);


            return BadRequest(string.Format("No Asset class found matching {0} ", code.Trim().ToUpper()));
        }


        [HttpPost]
        [Route("", Name = "CreateNewAssetClassification")]
        public async Task<IHttpActionResult> CreateNewAssetClass([FromBody] AssetClass newClassification)
        {
            string newLocation;
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for new Asset Class creation."
            });

            
            var existingAssetClass = await Task.FromResult(_repository.Retreive(ac => ac.Code.Trim() == newClassification.Code.Trim())
                                                                      .AsQueryable());

            if (existingAssetClass.Any())
                return ResponseMessage(new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.Conflict,
                                        ReasonPhrase = "Duplicate Asset Class found."
                });

           
            var isCreated = await Task.FromResult(_repository.Create(newClassification));

            if (!isCreated) return BadRequest("Unable to create Asset Class for:  " + newClassification.Code);

            var requestUri = ControllerContext.RequestContext.Url.Request.RequestUri.AbsoluteUri;
            newClassification.Url = requestUri + "/" + newClassification.Code.Trim();

            // Accomodate NUnit testing.
            if (ControllerContext.RouteData == null) 
            {
                newLocation = newClassification.Url;
                return Created(newLocation, newClassification); 
            }
            

            newLocation = Url.Link("CreateNewAssetClassification", new {newClassification.Code });
            return Created(newLocation, newClassification); // 201 status code

        }


        [HttpPut]
        [HttpPatch]
        [Route("{assetClassCode}")]
        public async Task<IHttpActionResult> UpdateAssetClass([FromBody] AssetClass updatedClassification, string assetClassCode)
        {
            var isUpdated = false;
            if (!ModelState.IsValid || assetClassCode.IsEmpty()) return ResponseMessage(new HttpResponseMessage
                                                                {
                                                                    StatusCode = HttpStatusCode.BadRequest,
                                                                    ReasonPhrase = "Invalid data or Asset Code received for Asset Class update."
                                                                });

            // Confirm received search-by code indeed matches correct asset class to be updated.
            var fetchedAssetClass = _repository.RetreiveById(updatedClassification.KeyId);
            var isCorrectAssetClass = fetchedAssetClass.Code.Trim() == updatedClassification.Code.Trim();

            if (isCorrectAssetClass)
            {
                isUpdated = await Task.FromResult(_repository.Update(updatedClassification, updatedClassification.KeyId));
                //isUpdated = await Task<bool>.Factory.StartNew(() => _repository.Update(updatedClassification, updatedClassification.KeyId));
            }


            if (isUpdated)
                return Ok(updatedClassification);

            return BadRequest("Unable to update Asset Class for: " + updatedClassification.Code);

        }
        


        // Require Guid as only acceptable parameter for deletes.
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            //var isDeleted = await Task<bool>.Factory.StartNew(() => _repository.Delete(id));
            var isDeleted = await Task.FromResult(_repository.Delete(id));

            if (isDeleted)
                return Ok("Delete successful");

            return BadRequest(string.Format("Unable to delete Asset Class, or id:  {0} , not found", id));

        }


    }
}
