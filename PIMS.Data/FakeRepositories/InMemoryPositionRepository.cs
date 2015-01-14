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
                            PurchaseDate = DateTime.UtcNow.AddYears(-3).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddMonths(-2).ToString("g"),
                            Quantity = 50,
                            MarketPrice = 109.13M,
                            Account = new AccountType
                                {
                                    Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc",
                                    AccountTypeDesc = "Roth-IRA", 
                                    KeyId = new Guid("33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc")
                                }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/YHO/Position/CMA",
                            InvestorKey = "Motheral",
                            PositionId = new Guid("0409fcc8-f12b-49f9-86fd-c572560cfe15"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-2).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddDays(-22).ToString("g"),
                            Quantity = 200,
                            MarketPrice = 109.13M,
                            Account = new AccountType
                                    {
                                       Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/0ff19b90-8628-41fd-bef3-6b3f237f19a6",
                                       AccountTypeDesc = "CMA", 
                                       KeyId = new Guid("0ff19b90-8628-41fd-bef3-6b3f237f19a6")
                                     }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/ETP/Position/IRRA",
                            InvestorKey = "Pinkston",
                            PositionId = new Guid("d427c6be-37e4-45f2-93d9-a97956fd3931"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-8).ToString("d"),
                            LastUpdate = DateTime.UtcNow.AddMonths(-1).ToString("g"),
                            Quantity = 300,
                            MarketPrice = 192.98M,
                            Account = new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/edc913b3-5ec2-46c7-90d1-38e88a86776b",
                                            AccountTypeDesc = "IRRA", 
                                            KeyId = new Guid("edc913b3-5ec2-46c7-90d1-38e88a86776b")
                                        }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/IRRA",
                            InvestorKey = "Asch",
                            PositionId = new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-1).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddDays(-9).ToString("g"),
                            Quantity = 50,
                            MarketPrice = 14.93M,
                            Account = new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/98228ef0-c3f1-43b1-b640-9c84e13bb99b",
                                            AccountTypeDesc = "IRRA", 
                                            KeyId = new Guid("98228ef0-c3f1-43b1-b640-9c84e13bb99b")
                                        }
                         },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/YHO/Position/Roth-IRA",
                            InvestorKey = "Motheral",
                            PositionId = new Guid("f5cf9bd6-e870-40f0-afb2-b1c7337896aa"), 
                            PurchaseDate = DateTime.UtcNow.AddYears(-3).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddMonths(-1).ToString("g"),
                            Quantity = 50,
                            MarketPrice = 109.13M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/7bfe50e5-b1e5-416c-862a-f5fde64ec7c9",
                                        AccountTypeDesc = "Roth-IRA", 
                                        KeyId = new Guid("7bfe50e5-b1e5-416c-862a-f5fde64ec7c9")
                                    }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("a03174a3-d5cc-49b1-99aa-62f993bff553"), 
                            PurchaseDate = DateTime.UtcNow.AddYears(-2).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddHours(-32).ToString("g"),
                            Quantity = 100,
                            MarketPrice = 142.93M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/be984208-a260-41fa-b125-e855aee37b4f",
                                        AccountTypeDesc = "ML-CMA", 
                                        KeyId = new Guid("be984208-a260-41fa-b125-e855aee37b4f")
                                    }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/Roth-IRA",
                            InvestorKey = "Pinkston", 
                            PositionId = new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-9).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.ToString("g"),
                            Quantity = 350,
                            MarketPrice = 82.93M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/4aac6a37-69fc-4a64-83c8-279562a7be50",
                                        AccountTypeDesc = "Roth-IRA", 
                                        KeyId = new Guid("4aac6a37-69fc-4a64-83c8-279562a7be50")
                                    }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/Roth-IRA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("03345f0f-00d2-4bb8-ad18-3928217337ea"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-4).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.ToString("g"),
                            Quantity = 75,
                            MarketPrice = 136.52M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/9adb088c-d42d-4f0b-979b-7a1c17276637",
                                        AccountTypeDesc = "Roth-IRA", 
                                        KeyId = new Guid("9adb088c-d42d-4f0b-979b-7a1c17276637")
                                    }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/IBM/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("04838c46-9574-41e3-a866-a686b79e53c7"), 
                            PurchaseDate = DateTime.UtcNow.AddMonths(-3).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.ToString("g"),
                            Quantity = 70,
                            MarketPrice = 142.52M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/0a37302b-1ffd-4f7e-8f9b-da24d55fde15",
                                        AccountTypeDesc = "ML-CMA", 
                                        KeyId = new Guid("0a37302b-1ffd-4f7e-8f9b-da24d55fde15")
                                    }
                        },
                        new Position
                        {
                            Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Position/ML-CMA",
                            InvestorKey = "Asch", 
                            PositionId = new Guid("2a64098f-e408-45ac-969f-6cfe566ef249"), 
                            PurchaseDate = DateTime.UtcNow.AddDays(-13).ToString("d"), 
                            LastUpdate = DateTime.UtcNow.AddHours(-10).ToString("g"),
                            Quantity = 250,
                            MarketPrice = 63.10M,
                            Account = new AccountType
                                    {
                                        Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/4a2e9df2-7de0-4285-9234-a193adcb5449",
                                        AccountTypeDesc = "ML-CMA", 
                                        KeyId = new Guid("4a2e9df2-7de0-4285-9234-a193adcb5449")
                                    }
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
