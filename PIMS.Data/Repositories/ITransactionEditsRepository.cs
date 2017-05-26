using System.Collections.Generic;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories

{
    public interface ITransactionEditsRepository<T> where T: Transaction
    {
        
        bool UpdateTransactions(IEnumerable<T> transactions);



    }

}
