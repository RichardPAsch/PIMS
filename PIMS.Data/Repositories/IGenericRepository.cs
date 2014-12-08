using System;
using System.Linq;
using System.Linq.Expressions;


namespace PIMS.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // UoW functionality is covered by NHibernate's ISession.
        IQueryable<T> RetreiveAll();
        IQueryable<T> Retreive(Expression<Func<T, bool>> predicate); 
        T RetreiveById(Guid key);
        bool Create(T newEntity);
        bool Delete(Guid idGuid);
        bool Update(T entity, object id);

        string UrlAddress { get; set; }

        // Use-case scenarios:
        // Retreive Assets by Investor
        // Retreive Classification by Asset
        // Retreive Profile by Asset
        // Retreive Positions by Investor.Assets
        // Retreive Incomes by Investor.Assets
        
    }
}
