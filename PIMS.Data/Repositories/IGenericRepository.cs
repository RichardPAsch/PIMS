using System;
using System.Linq;
using System.Linq.Expressions;


namespace PIMS.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Aggregate root.

        // UoW functionality is covered by NHibernate's ISession.

        string UrlAddress { get; set; }

        IQueryable<T> RetreiveAll();
        IQueryable<T> Retreive(Expression<Func<T, bool>> predicate); 
        T RetreiveById(Guid key);
        bool Create(T newEntity);
        bool Delete(Guid idGuid);
        bool Update(T entity, object id);
        
    }

}
