using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class IncomeRepository : IGenericRepository<Income>
    {
        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public IncomeRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }



        public IQueryable<Income> RetreiveAll()
        {
            var incomeQuery = (from income in _nhSession.Query<Income>() select income);
            return incomeQuery.AsQueryable();
        }


        public IQueryable<Income> Retreive(Expression<Func<Income, bool>> predicate)
        {
            try {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception) {
                return null;
            }
        }

       
        public Income RetreiveById(Guid key)
        {
            return _nhSession.Get<Income>(key);
        }


        public bool Create(Income newEntity)
        {
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Save(newEntity);
                    trx.Commit();
                }
                catch(Exception ex)
                {
                    //var debug = ex.InnerException;
                    return false;
                }

                return true;
            }
        }


        public bool Delete(Guid idGuid)
        {
            var deleteOk = true;
            var incomeToDelete = RetreiveById(idGuid);

            if (incomeToDelete == null)
                return false;


            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Delete(incomeToDelete);
                    trx.Commit();
                }
                catch (Exception) {
                    // TODO: Candidate for logging?
                    deleteOk = false;
                }
            }


            return deleteOk;
        }


        public bool Update(Income entity, object id)
        {
            entity.IncomeId = (Guid) id;
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    //var debug = ex.InnerException;
                    return false;
                }
            }

            return true;
        }

        
    }
}
