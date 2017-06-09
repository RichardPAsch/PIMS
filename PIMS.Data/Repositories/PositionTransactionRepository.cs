using System;
using NHibernate;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class PositionTransactionRepository
    {
        /*
            Used as a result of Position 'rollover' user-selected option, via menu Position - 'update - add'.
        */

        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public PositionTransactionRepository(ISessionFactory sessFactory) {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }



        public bool UpdateTransactionAndPosition(object[] sourceTrxPosData, object[] targetTrxPosData) {
            // Ex. rollover scenario : IRA (source) -> Roth-IRA (target).

            if (sourceTrxPosData[0].GetType() != typeof(Transaction) &&
                sourceTrxPosData[1].GetType() != typeof(Position) &&
                targetTrxPosData[0].GetType() != typeof(Transaction) &&
                targetTrxPosData[1].GetType() != typeof(Position))
            {
                return false;  
            }

            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Save(sourceTrxPosData[0]); 
                    _nhSession.Merge(sourceTrxPosData[1]); 
                    _nhSession.Save(targetTrxPosData[0]); 
                    _nhSession.Merge(targetTrxPosData[1]); 
                    trx.Commit();
                }
                catch (Exception ex) {
                    var res = ex.Message;
                    return false;
                }
            }
            return true;
        }



        //public bool UpdateTransactions(IEnumerable<Transaction> transactions) {
        //    var trx = _nhSession.BeginTransaction();
        //    foreach (var transaction in transactions) {
        //        try {
        //            _nhSession.SaveOrUpdate(transaction);
        //            trx.Commit();
        //        }
        //        catch (Exception ex) {
        //            var res = ex.Message;
        //            return false;
        //        }

        //        // Receive NHibernate error: "Cannot access a disposed object.\r\nObject name: 'AdoTransaction'."
        //        // after initial db commit, perhaps due to some exception, or use of 'using' clause ? Therefore, 
        //        // will have to create a new trx to continue.
        //        if (!trx.IsActive)
        //            trx = _nhSession.BeginTransaction();
        //    }

        //    trx.Dispose();
        //    return true;
        //}

    }
}


