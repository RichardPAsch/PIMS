using System;
using System.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{

    public class AssetRepository : IGenericRepository<Asset>
    {
        //public AssetRepository(string connectionString)
        public AssetRepository()
        {
            // to be implemented
        }



        //public Asset CreateAsset(Asset newAsset)
        //{
        //    throw new NotImplementedException();
        //}

        //public IQueryable<Asset> FetchAssets()
        //{
        //    throw new NotImplementedException();
        //}

        //public Asset FetchAsset(long id)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool UpdateAsset(long id)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool DeleteAsset(long id)
        //{
        //    throw new NotImplementedException();
        //}
        public IQueryable<Asset> RetreiveAll()
        {
            throw new NotImplementedException();
        }

        public Asset Retreive(object property)
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

        public bool Update(Asset entity)
        {
            throw new NotImplementedException();
        }
    }
}
