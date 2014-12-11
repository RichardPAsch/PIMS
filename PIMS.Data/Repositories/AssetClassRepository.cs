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

        private readonly ISessionFactory _sfFactory;
        public AssetClassRepository(ISessionFactory sfFactory)
        {
            if (sfFactory == null)
                throw new ArgumentNullException("sfFactory");

            _sfFactory = sfFactory;
        }

        public AssetClassRepository()
        {
          // TODO: Needed?
        }




        public virtual IQueryable<AssetClass> RetreiveAll()
        {
           using (var sess = _sfFactory.OpenSession())
            {
               var classsificationsQuery = (from classification in sess.Query<AssetClass>() select classification);
               return classsificationsQuery.ToList().AsQueryable();
            }
        }

        public IQueryable<AssetClass> Retreive(Expression<Func<AssetClass, bool>> predicate, IQueryable<object> data = null)
        {
            throw new NotImplementedException();
        }

        
        public AssetClass RetreiveById(Guid idGuid)
        {
            var listing = RetreiveAll();
            var filteredListing = listing
                .Where(ac => ac.KeyId == idGuid);
            return filteredListing.FirstOrDefault();
        }
        
        public bool Create(AssetClass newEntity)
        {
            var currListing = RetreiveAll().ToList();
            if (currListing.Any(ac => ac.Code.ToUpper().Trim() == newEntity.Code.ToUpper().Trim())) return false;

            using (var sess = _sfFactory.OpenSession())
            {
                using (var trx = sess.BeginTransaction())
                {
                    try
                    {
                        sess.Save(newEntity);
                        trx.Commit();
                    }
                    catch{
                        return false;
                    }
                }
            }

            return true;
        }
       
        public bool Delete(Guid cGuid) {

            var deleteOk = true;
            var assetClassToDelete = RetreiveById(cGuid);

            using (var sess = _sfFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try {
                        sess.Delete(assetClassToDelete);
                        trx.Commit();
                    }
                    catch (Exception) {
                        // TODO: Candidate for logging.
                        deleteOk = false;
                    }
                }
            }

            return deleteOk;
        }

        public bool Update(AssetClass entity, object id)
        {
            // Each AssetClass is unique, therefore we'll only need to
            // use the passed AssetClass's id to update the datastore.
            using (var sess = _sfFactory.OpenSession()) {

                using (var trx = sess.BeginTransaction()) {

                    try {
                        var listings = RetreiveAll().ToList();
                        var item = listings.Find(ac => ac.KeyId == (Guid)id); //entity.KeyId);
                        item.Code = entity.Code.Trim().ToUpper();
                        item.Description = entity.Description.Trim();

                        sess.Update(item);
                        trx.Commit();
                    }
                    catch {
                        return false;
                    }
                }
                return true;
            }
        }

        public string UrlAddress { get; set; }
    }
}
