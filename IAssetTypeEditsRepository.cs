using System.Collections.Generic;
using PIMS.Core.Models;



namespace PIMS.Data.Repositories
{
    public interface IAssetTypeEditsRepository<T> where T: Asset
    {
        
        bool UpdateAssetTypes(IEnumerable<T> assetTypes);

    }

}
