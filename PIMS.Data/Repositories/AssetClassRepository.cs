using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Impl;
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
               //using (var trx = sess.BeginTransaction()) {
               var classsificationsQuery = (from classification in sess.Query<AssetClass>() select classification);
               return classsificationsQuery.ToList().AsQueryable();
               //}
            }

        }

        public AssetClass RetreiveById(Guid idGuid)
        {
            var listing = RetreiveAll();
            var filteredListing = listing
                .Where(ac => ac.KeyId == idGuid);
            return filteredListing.FirstOrDefault();
        }
       
        public AssetClass Retreive(object assetClassCode)
        {
            var listing = RetreiveAll();
            var filteredListing = listing
                .Where(a => String.Equals(a.Code.Trim().ToUpper(), assetClassCode.ToString().Trim().ToUpper(), StringComparison.CurrentCultureIgnoreCase));
            return filteredListing.FirstOrDefault();
        }
        
        public bool Create(AssetClass newEntity)
        {
            using (var sess = _sfFactory.OpenSession())
            {
                using (var trx = sess.BeginTransaction())
                {
                    try
                    {
                        sess.Save(newEntity);
                        trx.Commit();
                    }
                    catch  
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Update(AssetClass entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid cGuid) {
            throw new NotImplementedException();
        }



        //public AssetClass Retreive(Expression<Func<AssetClass, int, bool>> predicate)
        //{
        //    //var z = RetreiveAll();
        //    //var a = new Guid("5db0cb9c-e1f8-4981-954b-96a79bce16e2");
        //    //var y = z.Where(c => c.ClassificationId == a); // ok, but why?

        //    var listing2 = RetreiveAll().Where(predicate);

        //    var listing = RetreiveAll();
        //    IQueryable<AssetClass> classification = listing.Where(c => c.ClassificationId == new Guid("5db0cb9c-e1f8-4981-954b-96a79bce16e2"));
        //    return classification.FirstOrDefault();//ok
        //}


    }
}
