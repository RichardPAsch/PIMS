using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{

    public class AssetRepository : IGenericRepository<Asset>
    {
        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public AssetRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }

       

        public IQueryable<Asset> RetreiveAll()
        {
            var assetsQuery = (from asset in _nhSession.Query<Asset>() select asset);
            return assetsQuery.AsQueryable();
        }


        public IQueryable<Asset> Retreive(Expression<Func<Asset, bool>> predicate)
        {
            try
            {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception ex)
            {
                var debug = ex.Message;
                return null;
            }
        }

        
        public Asset RetreiveById(Guid key)
        {
            return _nhSession.Get<Asset>(key);
        }


        public bool Create(Asset newEntity)
        {
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Save(newEntity);
                    trx.Commit();
                }
                catch (Exception ex) {
                    var debug = ex.InnerException;
                    return false;
                }

                return true;
            }
        }


        public bool Delete(Guid idGuid)
        {
            //TODO: All referenced objects must be cascade deleted.
            var deleteOk = true;
            var assetToDelete = RetreiveById(idGuid);

            if (assetToDelete == null)
                return false;

            
            using (var trx = _nhSession.BeginTransaction()) 
            {
                try {
                    _nhSession.Delete(assetToDelete);
                    trx.Commit();
                }
                catch (Exception ex) {
                    // TODO: Candidate for logging?
                    var debug = ex.InnerException;
                    deleteOk = false;
                }
            }


            return deleteOk;
        }


        public bool Update(Asset entity, object id)
        {
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch (Exception ex) {
                    //var debug = ex.InnerException;
                    return false;
                }
            }

            return true;
        }

        


    }
}
