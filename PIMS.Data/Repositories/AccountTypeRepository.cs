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
        
        private readonly ISessionFactory _nhSessFactory;
        public AccountTypeRepository(ISessionFactory nhSessFactory)
        {
            if (nhSessFactory == null) throw new ArgumentNullException("nhSessFactory");
            _nhSessFactory = nhSessFactory;
        }



        public virtual IQueryable<AccountType> RetreiveAll()
        {
            using (var sess = _nhSessFactory.OpenSession())
            {
                var atQuery = sess.Query<AccountType>().Select(at => at);
                return atQuery.ToList().AsQueryable();
            }
        }

        public IQueryable<AccountType> Retreive(Expression<Func<AccountType, bool>> predicate, IQueryable<object> data = null)
        {
            throw new NotImplementedException();
        }

        
       
        public AccountType RetreiveById(Guid idGuid)
        {
            var listing = RetreiveAll().Where(at => at.KeyId == idGuid);
            return listing.FirstOrDefault();
        }


       public bool Create(AccountType newEntity)
        {
            var currListing = RetreiveAll().ToList();
            if (currListing.Any(at => at.AccountTypeDesc.ToUpper().Trim() == newEntity.AccountTypeDesc.ToUpper().Trim())) return false;

            using (var sess = _nhSessFactory.OpenSession()) 
            {
                using (var trx = sess.BeginTransaction())
                {
                    try {
                        sess.Save(newEntity);
                        trx.Commit();
                    }
                    catch {
                        return false;
                    }
                }
            }

            return true;
        }
        

        
        public bool Update(AccountType entity, object id)
        {
            using (var sess = _nhSessFactory.OpenSession())
            {
                using (var trx = sess.BeginTransaction())
                {
                    try
                    {
                        var acctTypeItem = RetreiveAll().ToList().Find(at => at.KeyId == (Guid) id);
                        acctTypeItem.AccountTypeDesc = entity.AccountTypeDesc.Trim().ToUpper();

                        sess.Update(acctTypeItem);
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


        public bool Delete(Guid cGuid) {

            var deleteOk = true;
            var accountTypeToDelete = RetreiveById(cGuid);

            using (var sess = _nhSessFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try {
                        sess.Delete(accountTypeToDelete);
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



        public bool DeleteByType(string acctType) {

            var deleteOk = true;
            var accountTypeToDelete = RetreiveAll().FirstOrDefault(at => at.AccountTypeDesc.Trim() == acctType.Trim());

            using (var sess = _nhSessFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try {
                        sess.Delete(accountTypeToDelete);
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



    }

}

      




