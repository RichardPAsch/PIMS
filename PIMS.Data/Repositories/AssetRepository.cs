using System;
using System.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{

    public class AssetRepository : IAssetRepository
    {
        //public AssetRepository(string connectionString)
        public AssetRepository()
        {
            // to be implemented
        }



        public Asset CreateAsset(Asset newAsset)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Asset> FetchAssets()
        {
            throw new NotImplementedException();
        }

        public Asset FetchAsset(long id)
        {
            throw new NotImplementedException();
        }

        public bool UpdateAsset(long id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAsset(long id)
        {
            throw new NotImplementedException();
        }
    }
}
