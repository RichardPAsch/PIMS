using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class TransactionRepository : IGenericRepository<Transaction>, ITransactionEditsRepository<Transaction>
    {

        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public TransactionRepository(ISessionFactory sessFactory) {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }




        public IQueryable<Transaction> RetreiveAll() {
            var transactionQuery = (from trx in _nhSession.Query<Transaction>() select trx);
            return transactionQuery.AsQueryable();
        }


        public IQueryable<Transaction> Retreive(Expression<Func<Transaction, bool>> predicate) {
            try {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception) {
                return null;
            }
        }


        public Transaction RetreiveById(Guid key) {
            return _nhSession.Get<Transaction>(key);
        }


        public bool Create(Transaction newEntity) {
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


        public bool Delete(Guid idGuid) {
            var deleteOk = true;
            var positionToDelete = RetreiveById(idGuid);

            if (positionToDelete == null)
                return false;


            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Delete(positionToDelete);
                    trx.Commit();
                }
                catch (Exception) {
                    // TODO: Candidate for logging?
                    deleteOk = false;
                }
            }


            return deleteOk;
        }


        public bool Update(Transaction entity, object id)
        {
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch (Exception ex) {
                    var res = ex.Message;
                    return false;
                }
            }

            return true;
        }


      

        public bool UpdateTransactions(IEnumerable<Transaction> transactions)
        {
            var trx = _nhSession.BeginTransaction();
            foreach (var transaction in transactions) {
                try {
                    _nhSession.SaveOrUpdate(transaction);
                    trx.Commit(); 
                }
                catch (Exception ex) {
                    var res = ex.Message;
                    return false;
                }

                // Receive NHibernate error: "Cannot access a disposed object.\r\nObject name: 'AdoTransaction'."
                // after initial db commit, perhaps due to some exception, or use of 'using' clause ? Therefore, 
                // will have to create a new trx to continue.
                if (!trx.IsActive)
                    trx = _nhSession.BeginTransaction();
            }

            trx.Dispose();
            return true;
        }

    }
}

