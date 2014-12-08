using System;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{

    public class AssetRepository : IGenericRepository<Asset>
    {

        public AssetRepository()
        {
            
        }


        public IQueryable<Asset> RetreiveAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Asset> Retreive(Expression<Func<Asset, bool>> predicate)
        {
            throw new NotImplementedException();
        }

       
        public Asset RetreiveById(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool Create(Asset newEntity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid idGuid)
        {
            throw new NotImplementedException();
        }

        public bool Update(Asset entity, object id)
        {
            throw new NotImplementedException();
        }

        public string UrlAddress { get; set; }
    }
}
