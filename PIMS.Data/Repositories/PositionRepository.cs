using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class PositionRepository : IGenericRepository<Position>
    {
        // All Actions delegated to aggregate root (AssetRepository).

        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }

        public PositionRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }




        public IQueryable<Position> RetreiveAll()
        {
            var positionQuery = (from position in _nhSession.Query<Position>() select position);
            return positionQuery.AsQueryable();
        }


        public IQueryable<Position> Retreive(Expression<Func<Position, bool>> predicate)
        {
            try {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception) {
                return null;
            }
        }

      
        public Position RetreiveById(Guid key)
        {
            return _nhSession.Get<Position>(key);
        }


        public bool Create(Position newEntity)
        {
            using (var trx = _nhSession.BeginTransaction()) {
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


        public bool Delete(Guid idGuid)
        {
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

        
        public bool Update(Position entity, object id)
        {
            using (var trx = _nhSession.BeginTransaction())
            {
                try {
                    _nhSession.Merge(entity);
                    trx.Commit();
                }
                catch (Exception) {
                    return false;
                }
            }

            return true;
        }

        
    }
}
