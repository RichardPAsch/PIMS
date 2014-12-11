using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryPositionRepository : IGenericRepository<Position>
    {

        private readonly InMemoryAssetRepository _repoInMemoryAssetRepository = new InMemoryAssetRepository();


        public IQueryable<Position> RetreiveAll() {return null;}

        public IQueryable<Position> Retreive(Expression<Func<Position, bool>> predicate, IQueryable<object> data = null)
        {
            var filteredAsset = (IQueryable<Asset>) data;
            return filteredAsset != null ? filteredAsset.First().Positions.AsQueryable().Where(predicate) : null;
        }


        public Position RetreiveById(Guid key)
        {
            throw new NotImplementedException();
        }
        
        public bool Create(Position newEntity)
        {
            // Mimic NHibernate trx Save().
            var existingAsset = _repoInMemoryAssetRepository.Retreive(a => a.Investor.LastName == "Asch" && a.Profile.TickerSymbol == "VNR").AsQueryable();
            existingAsset.First().Positions.Add(newEntity);
            
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
