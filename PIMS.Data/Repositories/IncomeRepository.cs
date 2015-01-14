using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class IncomeRepository : IGenericRepository<Income>
    {

        private readonly ISessionFactory _sfFactory;

        public IncomeRepository(ISessionFactory sfFactory)
        {
            if (sfFactory == null)
                throw new ArgumentNullException("sfFactory");

            _sfFactory = sfFactory;
        }


        public IQueryable<Income> RetreiveAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Income> Retreive(Expression<Func<Income, bool>> predicate)
        {
            throw new NotImplementedException();
        }

       
        public Income RetreiveById(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool Create(Income newEntity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid idGuid)
        {
            throw new NotImplementedException();
        }

        public bool Update(Income entity, object id)
        {
            throw new NotImplementedException();
        }

        public string UrlAddress { get; set; }
    }
}
