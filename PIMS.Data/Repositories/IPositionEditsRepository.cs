

namespace PIMS.Data.Repositories
{
    public interface IPositionEditsRepository<T> where T : class
    {
        // UoW functionality is covered by NHibernate's ISession.

        //IQueryable<T> RetreiveAll();
        //IQueryable<T> Retreive(Expression<Func<T, bool>> predicate);
        //T RetreiveById(Guid key);
        //bool Create(T newEntity);
        //bool Delete(Guid idGuid);
        bool UpdatePositions(T entity, T entity2);

        bool UpdateCreatePositions(T entity, T entity2);

    }

}
