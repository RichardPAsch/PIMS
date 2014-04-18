using System;
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


        public HttpResponseMessage Get(Guid assetId)
        {
            return null;
        }

    }
}
