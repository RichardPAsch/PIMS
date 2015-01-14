using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class PositionRepository : IGenericRepository<Position>
    {

        private readonly ISessionFactory _sfFactory;

        public PositionRepository(ISessionFactory sfFactory)
        {
            if (sfFactory == null)
                throw new ArgumentNullException("sfFactory");

            _sfFactory = sfFactory;
        }




        public IQueryable<Position> RetreiveAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Position> Retreive(Expression<Func<Position, bool>> predicate)
        {
            throw new NotImplementedException();
        }

      
        public Position RetreiveById(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool Create(Position newEntity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid idGuid)
        {
            throw new NotImplementedException();
        }

        
        public bool Update(Position entity, object id)
        {
            throw new NotImplementedException();
        }

        public string UrlAddress { get; set; }
    }
}
