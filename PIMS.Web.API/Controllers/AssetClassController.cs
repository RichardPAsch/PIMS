using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using PIMS.Core.Models;
using PIMS.Data.FakeRepositories;
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

        // TODO: Why do we get a relative URL here (commented code for uri); use of "newClassification.Code" - correct?


        // GET api/accounttype

        private IHttpActionResult GetAssetClass(string assetClassCode)
        {
            // TODO: Implement in IGenericRepository
            //var tempRepo = new InMemoryAssetClassRepository();
            //var classification = tempRepo.TestFind(ac => ac.Code.ToUpper().Trim() == assetClassCode.ToUpper().Trim());

            var classification = _repository.RetreiveAll().FirstOrDefault(ac => ac.Code.ToUpper().Trim() == assetClassCode.ToUpper().Trim());

            return classification == null
                ? (IHttpActionResult)NotFound()
                : Ok(classification);
        }
        
        

        
        [HttpGet]
        [Route("")]
        public async Task<IQueryable<AssetClass>> GetAll() {
            return await Task<IQueryable<AssetClass>>.Factory.StartNew(() => _repository.RetreiveAll());
        }


        // GET api/AssetClass/CS
        [HttpGet]
        [Route("{assetClassCode?}")]
        public async Task<IHttpActionResult> GetByCode(string assetClassCode) 
        {
            return await Task.FromResult(GetAssetClass(assetClassCode));
        }
        

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateNewAssetClass([FromBody] AssetClass newClassification) 
        {
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for Asset Class."
            });

            var existingAssetClass = await Task<AssetClass>.Factory
                                                           .StartNew(() => _repository.RetreiveAll()
                                                                                      .FirstOrDefault(ac => String.Equals(ac.Code.Trim(),
                                                                                         newClassification.Code.Trim(),
                                                                                         StringComparison.CurrentCultureIgnoreCase)));

            if (existingAssetClass != null)
                return ResponseMessage(new HttpResponseMessage {
                                                StatusCode = HttpStatusCode.Conflict,
                                                ReasonPhrase = "Duplicate Asset Class found."
                });

            var isCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(newClassification));

            //var x = new UrlHelper(new HttpRequestMessage(HttpMethod.Post))
#if DEBUG
            var newLocation = "http://localhost/Pims.Web.Api/api/AssetClass/" + newClassification.Code.Trim();
#else
            var newLocation = ControllerContext.Request.RequestUri.AbsoluteUri + "/" + newClassification.Code.Trim();
#endif
       
            if (isCreated)
                return Created(newLocation, newClassification); // 201 status code

            
            return BadRequest("Unable to create Asset Class " + newClassification.Code);
        }


        [HttpPut]
        [Route("{assetClassCode}")]
        public async Task<IHttpActionResult> UpdateAssetClass([FromBody] AssetClass updatedClassification, string assetClassCode)
        {
            // No UOW repository or interface is necessary, as "Classifcation" involves no
            // logical transactions or object graphs, when used by itself.
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for Asset Class."
            });

            // Confirm received code matches asset class to be updated.
            var isCorrectAssetClass = _repository.RetreiveAll().Any(ac => ac.Code.ToUpper().Trim() == assetClassCode.ToUpper().Trim());
            var isUpdated = true;

            if (isCorrectAssetClass)
            {
                isUpdated = await Task<bool>.Factory.StartNew(() => _repository.Update(updatedClassification, updatedClassification.KeyId));
            }
                

            if (isUpdated)
                return Ok(updatedClassification);

            return BadRequest("Unable to update Asset Class " + updatedClassification.Code);

        }


        // Guid parameter name (id) must match RouteTemplate name in WebApiConfig.
        // Require Guid as only acceptable parameter for deletes.
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            var isDeleted = await Task<bool>.Factory.StartNew(() => _repository.Delete(id));

            if(isDeleted)
                return Ok("Delete successful");

            return BadRequest(string.Format("Unable to delete, or {0} not found", id));
           
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



 
    }
}
