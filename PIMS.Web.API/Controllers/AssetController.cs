using System.Net;
using System.Net.Http;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    public class AssetController : ApiController
    {
        private IGenericRepository<Asset> _repository;

        public AssetController(IGenericRepository<Asset> repository )
        {
            _repository = repository;
        }



        public HttpResponseMessage Get(HttpRequestMessage req, string ticker)
        {
            return req.CreateResponse(HttpStatusCode.OK);
        }

    }
}
