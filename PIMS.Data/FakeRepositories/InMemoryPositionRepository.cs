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
        private IGenericRepository<AccountType> repositoryAccountType = new InMemoryAccountTypeRepository();

        
        public string UrlAddress { get; set; }
       

        // Retreive() used by Aggregate root repository linking, when fetching child aggregate Positions.
        public IQueryable<Position> RetreiveAll()
        {
            var positionListing = new List<Position>
                {
                    new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Roth-IRA",
                            InvestorKey = "Asch",
                            PositionId = new Guid("96b5f8bd-21f2-4921-bc5b-bcff6e39cbdb"), 
                            PurchaseDate = DateTime.UtcNow.AddYears(-3), 
                            LastUpdate = DateTime.UtcNow.AddMonths(-2),
                            Quantity = 50,
                            MarketPrice = 109.13M,
                            Account = repositoryAccountType.RetreiveById(new Guid("96b5f8bd-21f2-4921-bc5b-bcff6e39cbdb"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/YHO/Position/CMA",
                            InvestorKey = "Motheral",
                            PositionId = new Guid("0409fcc8-f12b-49f9-86fd-c572560cfe15"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-2), 
                            LastUpdate = DateTime.UtcNow.AddDays(-22),
                            Quantity = 200,
                            MarketPrice = 109.13M,
                            Account = repositoryAccountType.RetreiveById(new Guid("0409fcc8-f12b-49f9-86fd-c572560cfe15"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/ETP/Position/IRRA",
                            InvestorKey = "Pinkston",
                            PositionId = new Guid("d427c6be-37e4-45f2-93d9-a97956fd3931"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-8),
                            LastUpdate = DateTime.UtcNow.AddMonths(-1),
                            Quantity = 300,
                            MarketPrice = 192.98M,
                            Account = repositoryAccountType.RetreiveById(new Guid("d427c6be-37e4-45f2-93d9-a97956fd3931"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/IRRA",
                            InvestorKey = "Asch",
                            PositionId = new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-1), 
                            LastUpdate = DateTime.UtcNow.AddDays(-9),
                            Quantity = 50,
                            MarketPrice = 14.93M,
                            Account = repositoryAccountType.RetreiveById(new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/YHO/Position/Roth-IRA",
                            InvestorKey = "Motheral",
                            PositionId = new Guid("f5cf9bd6-e870-40f0-afb2-b1c7337896aa"), 
                            PurchaseDate = DateTime.UtcNow.AddYears(-3), 
                            LastUpdate = DateTime.UtcNow.AddMonths(-1),
                            Quantity = 50,
                            MarketPrice = 109.13M,
                            Account = repositoryAccountType.RetreiveById(new Guid("f5cf9bd6-e870-40f0-afb2-b1c7337896aa"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("a03174a3-d5cc-49b1-99aa-62f993bff553"), 
                            PurchaseDate = DateTime.UtcNow.AddYears(-2), 
                            LastUpdate = DateTime.UtcNow.AddHours(-32),
                            Quantity = 100,
                            MarketPrice = 142.93M,
                            Account = repositoryAccountType.RetreiveById(new Guid("a03174a3-d5cc-49b1-99aa-62f993bff553"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/Roth-IRA",
                            InvestorKey = "Pinkston", 
                            PositionId = new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-9), 
                            LastUpdate = DateTime.UtcNow,
                            Quantity = 350,
                            MarketPrice = 82.93M,
                            Account = repositoryAccountType.RetreiveById(new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/Roth-IRA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("03345f0f-00d2-4bb8-ad18-3928217337ea"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-4), 
                            LastUpdate = DateTime.UtcNow,
                            Quantity = 75,
                            MarketPrice = 136.52M,
                            Account = repositoryAccountType.RetreiveById(new Guid("03345f0f-00d2-4bb8-ad18-3928217337ea"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("04838c46-9574-41e3-a866-a686b79e53c7"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-3), 
                            LastUpdate = DateTime.UtcNow,
                            Quantity = 70,
                            MarketPrice = 142.52M,
                            Account = repositoryAccountType.RetreiveById(new Guid("04838c46-9574-41e3-a866-a686b79e53c7"))
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("2a64098f-e408-45ac-969f-6cfe566ef249"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-13), 
                            LastUpdate = DateTime.UtcNow.AddHours(-10),
                            Quantity = 250,
                            MarketPrice = 63.10M,
                            Account = repositoryAccountType.RetreiveById(new Guid("2a64098f-e408-45ac-969f-6cfe566ef249"))
                        }
                };

            return positionListing.AsQueryable();

        }
        
        public IQueryable<Position> Retreive(Expression<Func<Position, bool>> predicate)
        {
            return RetreiveAll().Where(predicate);
        }
        
        public Position RetreiveById(Guid key)
        {
            return null;
        }
        

        // The following 3 methods are superceeded by Aggregate functionality located with the
        // aggregate root, e.g., AssetRepository.
        public bool Create(Position newEntity)
        {
            return false;
        }

        public bool Delete(Guid idGuid)
        {
            return false;
        }

        public bool Update(Position entity, object id)
        {
            return false;
        }

        
    }

}
