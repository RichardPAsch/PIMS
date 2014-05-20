using System;
using System.Linq;


namespace PIMS.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // UoW functionality is covered by NHibernate's ISession.
        IQueryable<T> RetreiveAll();
        T Retreive(object property);
        T RetreiveById(Guid key);
        bool Create(T newEntity);
        bool Delete(Guid idGuid);

        // Need greater type flexibility for id, i.e., string, Guid.
        bool Update(T entity, object id);

        // bool Save()
    }
}
