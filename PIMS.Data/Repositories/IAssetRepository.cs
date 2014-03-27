using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public interface IAssetRepository
    {
        Asset CreateAsset(Asset newAsset);

        IQueryable<Asset> FetchAssets();

        Asset FetchAsset(long id);

        bool UpdateAsset(long id);

        // No hard deletion, just status change: (I)nactive
        bool DeleteAsset(long id);



    }
}
