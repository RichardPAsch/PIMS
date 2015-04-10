using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;



namespace PIMS.Data.Repositories
{
    public class AssetClassRepository : IGenericRepository<AssetClass>
    {
        // Notes:
        // 1. For entities containing object graphs, we'll want to transform the referenced object(s) via "Select" into anonymous object(s).
        //    In this controller, this will not be necessary.
        // 2. Consider use of ModelFactory(ies) when replicating code involving object creations. see Models vs. Entities - SWildermuth.
        // 3. Controllers provide presentation specifications/criteria (via Linq - FIND/SINGLE, etc) to repositories, such that they (repos)
        //    can get the data from a db, and populate a model to send to a view for rendering.
        // 4. Controllers generally contain business logic in their actions.



       
        //public string _connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";

        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public AssetClassRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            // Allow NH to determine when to clear the session.
            _nhSession.FlushMode = FlushMode.Auto;
        }

        


        public IQueryable<AssetClass> RetreiveAll()
        {
            var classsificationsQuery = (from classification in _nhSession.Query<AssetClass>() select classification);
            return classsificationsQuery.AsQueryable();
        }


        public AssetClass RetreiveById(Guid idGuid)
        {
            return _nhSession.Get<AssetClass>(idGuid);
        }


        public IQueryable<AssetClass> Retreive(Expression<Func<AssetClass, bool>> predicate)
        {
            return RetreiveAll().Where(predicate);
        }
        

        public bool Create(AssetClass newEntity)
        {
            using (var trx = _nhSession.BeginTransaction())
            {
                try {
                    _nhSession.Save(newEntity);
                    trx.Commit();
                }
                catch {
                    return false;
                }

                return true;
            }

        }


        public bool Update(AssetClass entity, object id)
        {
            using (var trx = _nhSession.BeginTransaction())
            {
                try
                {
                    // NH session already contains a persistent cache instance with the same identifier, therefore we'll save our modifications
                    // without knowing about the state of a session, using Merge(). Otherwise use of Update() generates an error.
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch(Exception)
                {
                    //var debug = ex.Message;
                    return false;
                }
            }

            return true;
        }
        

        public bool Delete(Guid cGuid)
        {
            var deleteOk = true;
            var assetClassToDelete = RetreiveById(cGuid);

            if(assetClassToDelete == null)
                return false;


            using (var trx = _nhSession.BeginTransaction())
            {
                try {
                    _nhSession.Delete(assetClassToDelete);
                    trx.Commit();
                }
                catch (Exception) {
                    // TODO: Candidate for logging?
                    deleteOk = false;
                }
            }


            return deleteOk;
        }

       

        
    }
}
