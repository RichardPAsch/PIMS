using System;
using System.Collections.Generic;
using NHibernate;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class AssetTypeEditsRepository : IAssetTypeEditsRepository<Asset>
    {
        private readonly ISession _nhSession;

        public AssetTypeEditsRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }



        public bool UpdateAssetTypes(IEnumerable<Asset> assetTypes)
        {
            var trx =  _nhSession.BeginTransaction();
            foreach (var type in assetTypes) {
                try {
                    _nhSession.Merge(type);
                    trx.Commit(); 
                }
                catch (Exception ex) {
                    var res = ex.Message;
                    return false;
                }

                if (!trx.IsActive)
                    trx =  _nhSession.BeginTransaction();
            }

            trx.Dispose();
            return true;
        }
    }
}
