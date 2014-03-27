using System;
using System.Linq;


namespace PIMS.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // UoW functionality is covered by NHibernate's ISession.

        IQueryable<T> RetreiveAll();
        //T Retreive(Expression<Func<T, int, bool>> predicate);
        T Retreive(object property);
        T RetreiveById(Guid key);
        bool Create(T newEntity);
        bool Delete(Guid idGuid);
        bool Update(T entity);

        // bool Save()
    }
}
