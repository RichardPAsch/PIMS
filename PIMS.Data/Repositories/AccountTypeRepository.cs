using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class AccountTypeRepository : IGenericRepository<AccountType>
    {
        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public AccountTypeRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }



        public virtual IQueryable<AccountType> RetreiveAll()
        {
            var accountTypeQuery = (from accountType in _nhSession.Query<AccountType>() select accountType);
            return accountTypeQuery.AsQueryable();
        }


        public AccountType RetreiveById(Guid idGuid)
        {
            return _nhSession.Get<AccountType>(idGuid);
        }


        public IQueryable<AccountType> Retreive(Expression<Func<AccountType, bool>> predicate)
        {
            return RetreiveAll().Where(predicate);
        }
        

        public bool Create(AccountType newEntity)
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
        
        
        public bool Update(AccountType entity, object id)
        {
            using (var trx = _nhSession.BeginTransaction())
            {
                try
                {
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch (Exception) {
                    return false;
                }
            }

            return true;
        }
        


        public bool Delete(Guid cGuid) {

            var deleteOk = true;
            var accountTypeToDelete = RetreiveById(cGuid);

            if (accountTypeToDelete == null)
                return false;


            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Delete(accountTypeToDelete);
                    trx.Commit();
                }
                catch (Exception) {
                    deleteOk = false;
                }
            }

            return deleteOk;
        }


       
    }

}

      




