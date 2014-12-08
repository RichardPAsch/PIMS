using System;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryPositionRepository : IGenericRepository<Position>
    {
        private readonly IPimsIdentityService _pimsIdentityService;
        public InMemoryPositionRepository(IPimsIdentityService identityService)
        {
            _pimsIdentityService = identityService;
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
            var investorName = _pimsIdentityService.CurrentUser;
            var newPosition = new Position
                              {
                                  PositionId = Guid.NewGuid(),
                                  PurchaseDate = DateTime.UtcNow.ToString("d"),
                                  Quantity = 100,

                              };
            return true;
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
