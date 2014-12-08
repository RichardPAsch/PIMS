using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAssetRepository : IGenericRepository<Asset>
    {
        

        public static string UrlAddress { get; set; }
        private readonly InMemoryInvestorRepository _repoInMemoryInvestorRepository = new InMemoryInvestorRepository();
        private readonly InMemoryProfileRepository _repoInMemoryProfileRepository = new InMemoryProfileRepository();


        string IGenericRepository<Asset>.UrlAddress
        {
            get { return UrlAddress; }
            set { UrlAddress =  value ; }
        }



        public IQueryable<Asset> RetreiveAll()
        {
            var assetListing = new List<Asset>
                            {
                                new Asset
                                {
                                    Url = UrlAddress,
                                    AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
                                    Investor =  _repoInMemoryInvestorRepository.Retreive(i => i.LastName == "Asch").Single(),
                                    AssetClass = new AssetClass
                                                    {
                                                        Url = "",
                                                        Code = "CB", 
                                                        Description = "Corporate Bond", 
                                                        KeyId = new Guid("75a3983f-da08-4ee6-bfbb-664088133483")
                                                    },
                                    Revenue = new List<Income>
                                              {
                                                 new Income
                                                    {
                                                        Url = "",
                                                        Actual = 56.11M, 
                                                        DateRecvd = new DateTime(2014,04,17,17,20,0).ToString("g"),
                                                        IncomeId = new Guid("34b9d8cb-2952-4409-87bc-91470d00b763"), 
                                                        Projected = 52.56M,
                                                        Account = "Roth-IRA"
                                                    },
                                                 new Income
                                                    {
                                                        Url = "",// UrlAddress + "/AAPL/Income",
                                                        Actual = 73.19M, 
                                                        DateRecvd = new DateTime(2014,05,15,12,29,0).ToString("g"),
                                                        IncomeId = new Guid("229ce910-e4f7-41e3-a93e-9d674ffb07af"), 
                                                        Projected = 72.56M,
                                                        Account = "ML-CMA"
                                                    } ,
                                                 new Income
                                                    {
                                                        Url = "",// UrlAddress + "/AAPL/Income",
                                                        Actual = 53.19M, 
                                                        DateRecvd = new DateTime(2014,07,10,18,09,0).ToString("g"),
                                                        IncomeId = new Guid("b0d08fe8-200f-450c-8761-6a9ff7da177d"), 
                                                        Projected = 72.56M,
                                                        Account = "Roth-IRA"
                                                    } 
                                              },
                                    Positions = new List<Position>
                                                {
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Position",
                                                        PositionId = new Guid("96b5f8bd-21f2-4921-bc5b-bcff6e39cbdb"), 
                                                        PurchaseDate = DateTime.UtcNow.AddYears(-3).ToString("d"), 
                                                        Quantity = 50,
                                                        UnitCost = 109.13M,
                                                        Account = new AccountType
                                                                {
                                                                    Url = "",
                                                                    AccountTypeDesc = "Roth-IRA", 
                                                                    KeyId = new Guid("be364e77-2e99-442c-904d-f50a8781a749")
                                                                }
                                                    },
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Position",
                                                        PositionId = new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa"), 
                                                        PurchaseDate = DateTime.UtcNow.AddYears(-2).ToString("d"), 
                                                        Quantity = 100,
                                                        UnitCost = 142.94M,
                                                        Account = new AccountType
                                                                {
                                                                    Url = "",
                                                                    AccountTypeDesc = "ML-CMA", 
                                                                    KeyId = new Guid("89f93743-dffa-43e5-b775-3b91fe9839db")
                                                                }
                                                    }
                                                },
                                    Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "IBM").Single()
                                },
                                new Asset
                                {
                                    Url = UrlAddress,
                                    AssetId = new Guid("f9cea918-798b-4323-884f-917090b23858"),
                                    Investor = _repoInMemoryInvestorRepository.Retreive(i => i.LastName == "Motheral").Single(),
                                    AssetClass = new AssetClass
                                                    {
                                                        Url = "", //UrlAddress.Replace("Asset","AssetClass") + "/CS",
                                                        Code = "CS", 
                                                        Description = "Common Stock", 
                                                        KeyId = new Guid("f63ae3d2-6e5d-425a-8424-ffc3d375eec1")
                                                    },
                                    Revenue = new List<Income>
                                              {
                                                 new Income
                                                    {
                                                        Url = "", //UrlAddress + "/YHO/Income",
                                                        Actual = 5.31M, 
                                                        DateRecvd = new DateTime(2012,01,10,08,09,0).ToString("g"), 
                                                        IncomeId = new Guid("abe7c0c6-e691-4c34-804f-0fb422856999"), 
                                                        Projected = 62.26M,
                                                        Account = "ML-CMA"
                                                    } 
                                              },
                                    Positions = new List<Position>
                                                {
                                                    new Position
                                                    {
                                                        Url = "" , //UrlAddress + "/YHO/Position",
                                                        PositionId = new Guid("0409fcc8-f12b-49f9-86fd-c572560cfe15"), 
                                                        PurchaseDate = DateTime.UtcNow.AddDays(-2).ToString("d"), 
                                                        Quantity = 200,
                                                        UnitCost = 109.13M,
                                                        Account = new AccountType
                                                                    {
                                                                        Url = "", //UrlAddress.Replace("Asset","AccountType") + "/IRA",
                                                                        AccountTypeDesc = "CMA", 
                                                                        KeyId = new Guid("fed3a363-2280-4d64-8640-440963290744")
                                                                    }
                                                    }
                                                },  
                                    Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "YHO").Single()
                                },
                                new Asset
                                {
                                    Url = UrlAddress,
                                    AssetId = new Guid("cc950e42-1f08-49b9-880b-a35f4d60b317"),
                                    Investor = _repoInMemoryInvestorRepository.Retreive(i => i.LastName == "Pinkston").Single(),
                                    AssetClass = new AssetClass
                                                    {
                                                        Url = "", //UrlAddress.Replace("Asset","AssetClass") + "/MLP",
                                                        Code = "MLP", 
                                                        Description = "Master Limited Partnership", 
                                                        KeyId = new Guid("25c33d28-327c-4cbb-858e-1d85224e68c9")
                                                    },
                                     Revenue = new List<Income>
                                              {
                                                 new Income
                                                    {
                                                        Url = "", //UrlAddress + "/ETP//Income",
                                                        Actual = 191.09M, 
                                                        DateRecvd = new DateTime(2014,01,7,10,59,0).ToString("g"), 
                                                        IncomeId = new Guid("4adbb38c-0811-45a2-9908-f0cdada6cdf1"), 
                                                        Projected = 198.44M,
                                                        Account = "IRRA"
                                                    } 
                                              },
                                    Positions = new List<Position>
                                                {
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/ETP/Position",
                                                        PositionId = new Guid("d427c6be-37e4-45f2-93d9-a97956fd3931"), 
                                                        PurchaseDate = DateTime.UtcNow.AddDays(-8).ToString("d"),
                                                        Quantity = 300,
                                                        UnitCost = 192.98M,
                                                        Account = new AccountType
                                                                    {
                                                                        Url = "", //UrlAddress.Replace("Asset","AccountType") + "/ML-CMA",
                                                                        AccountTypeDesc = "IRRA", 
                                                                        KeyId = new Guid("63fe441d-7fa4-4366-92b9-d0cda2d0bdbf")
                                                                    }
                                                    }
                                                },
                                    Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "ETP").Single()                                                    
                                },
                                new Asset
                                {
                                    Url = UrlAddress,
                                    AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
                                    Investor = _repoInMemoryInvestorRepository.Retreive(i => i.LastName == "Asch").Single(),
                                    AssetClass = new AssetClass
                                                    {
                                                        Url = "", //UrlAddress.Replace("Asset","AssetClass") + "/ETF",
                                                        Code = "ETF", 
                                                        Description = "Exchange Traded Fund", 
                                                        KeyId = new Guid("28f8a3d2-42a2-4d39-b503-54a9a3143dcb")
                                                    },
                                    Revenue = new List<Income>
                                              {
                                                 new Income
                                                    {
                                                        Url = "", //UrlAddress + "/VNR/Income",
                                                        Actual = 216.91M, 
                                                        DateRecvd = new DateTime(2014,04,30,19,50,0).ToString("g"), 
                                                        IncomeId = new Guid("445f7e72-2018-4762-9924-145c1d72488b"), 
                                                        Projected = 212.56M,
                                                        Account = "IRRA"
                                                    } 
                                              },
                                    Positions = new List<Position>
                                                {
                                                    new Position
                                                    {
                                                       Url = "", //UrlAddress + "/VNR/Position",
                                                       PositionId = new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8"), 
                                                       PurchaseDate = DateTime.UtcNow.AddDays(-1).ToString("d"), 
                                                       Quantity = 50,
                                                       UnitCost = 14.93M,
                                                       Account = new AccountType
                                                                    {
                                                                        Url = "", //UrlAddress.Replace("Asset","AccountType") + "/401(k)",
                                                                        AccountTypeDesc = "IRRA", 
                                                                        KeyId = new Guid("776286c8-6e5f-4496-a135-8e69bb8b81b4")
                                                                    }
                                                    }
                                                },
                                    Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "VNR").Single()
                                },
                                new Asset
                                {
                                    Url = UrlAddress,
                                    AssetId = new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa"),
                                    Investor = _repoInMemoryInvestorRepository.Retreive(i => i.LastName == "Asch").Single(),
                                    AssetClass = new AssetClass
                                                    {
                                                        Url = "", //UrlAddress.Replace("Asset","AssetClass") + "/CB",
                                                        Code = "CB", 
                                                        Description = "Corporate Bond", 
                                                        KeyId = new Guid("da329599-a301-411d-8167-b8e571a531d1")
                                                    },
                                    Revenue = new List<Income>
                                              {
                                                 new Income
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Income",
                                                        Actual = 56.29M, 
                                                        DateRecvd = new DateTime(2013,12,19,09,52,0).ToString("g"), 
                                                        IncomeId = new Guid("6389dbd1-174c-436e-a7d5-327ba6ea1980"), 
                                                        Projected = 52.56M,
                                                        Account = "Roth-IRA"
                                                    } 
                                              },
                                    Positions = new List<Position>
                                                {
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Position",
                                                        PositionId = new Guid("96b5f8bd-21f2-4921-bc5b-bcff6e39cbdb"), 
                                                        PurchaseDate = DateTime.UtcNow.AddYears(-3).ToString("d"), 
                                                        Quantity = 50,
                                                        UnitCost = 109.13M,
                                                        Account = new AccountType
                                                                    {
                                                                        Url = "", //UrlAddress.Replace("Asset","AccountType") + "/IRA",
                                                                        AccountTypeDesc = "Roth-IRA", 
                                                                        KeyId = new Guid("469491d0-3353-4d62-b4cb-f79ab0e2c9e0")
                                                                    }
                                                    },
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Position",
                                                        PositionId = new Guid("a03174a3-d5cc-49b1-99aa-62f993bff553"), 
                                                        PurchaseDate = DateTime.UtcNow.AddYears(-2).ToString("d"), 
                                                        Quantity = 100,
                                                        UnitCost = 142.93M,
                                                        Account = new AccountType
                                                                        {
                                                                            Url = "", //UrlAddress.Replace("Asset","AccountType") + "/IRA",
                                                                            AccountTypeDesc = "Roth-IRA", 
                                                                            KeyId = new Guid("09b1f08a-095c-4eb5-9add-8de106894945")
                                                                        }
                                                    },
                                                    new Position
                                                    {
                                                        Url = "", //UrlAddress + "/AAPL/Position",
                                                        PositionId = new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa"), 
                                                        PurchaseDate = DateTime.UtcNow.AddMonths(-9).ToString("d"), 
                                                        Quantity = 350,
                                                        UnitCost = 82.93M,
                                                        Account = new AccountType
                                                                        {
                                                                            Url = "", //UrlAddress.Replace("Asset","AccountType") + "/IRA",
                                                                            AccountTypeDesc = "Roth-IRA", 
                                                                            KeyId = new Guid("b7b010e1-e880-411c-9f0b-d135fcc7b2d5")
                                                                        }
                                                    }
                                                },
                                     Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "AAPL").Single()
                                }

                    };

            // Initialize URLs.
            foreach (var item in assetListing)
            {
                item.Url = "http://localhost/Pims.Web.Api/api/Asset/" + item.Profile.TickerSymbol.ToUpper().Trim();
                item.Investor.Url = "http://localhost/Pims.Web.Api/api/Investor/" + item.Investor.FirstName + item.Investor.MiddleInitial + item.Investor.LastName;
                item.AssetClass.Url = "http://localhost/Pims.Web.Api/api/AssetClass/" + item.AssetClass.Code.Trim().ToUpper();
                item.Profile.Url = "http://localhost/Pims.Web.Api/api/Profile/" + item.Profile.TickerSymbol.Trim().ToUpper();
                foreach (var subitem in item.Positions) {
                    subitem.Url = "http://localhost/Pims.Web.Api/api/Position/" + subitem.PositionId;
                    subitem.Account.Url = "http://localhost/Pims.Web.Api/api/Position/Account/" + Guid.NewGuid();
                }
                foreach (var subitem in item.Revenue)
                    subitem.Url = "http://localhost/Pims.Web.Api/api/Income/" + subitem.IncomeId;
            }

            return assetListing.AsQueryable();
        }
        
        public IQueryable<Asset> Retreive(Expression<Func<Asset, bool>> predicate)
       {
           return RetreiveAll().Where(predicate);
       }
        
        public Asset RetreiveById(Guid key) {
            //try {
            //    selectedAsset = RetreiveAll().First(a => a.InvestorId == key);
            //}
            //catch (Exception) {
            //    return null;
            //}

            //return selectedAsset;
            return null;
        }

        public bool Create(Asset newEntity)
        {
            var currListing = RetreiveAll().ToList();

            currListing.Add(newEntity);
            return true;
        }

        // TODO: implementation pending
        public bool Delete(Guid idGuid)
        {
            //
            var assets = RetreiveAll().ToList();
            var asset = Retreive(a => a.AssetId == idGuid).FirstOrDefault();
            assets.Remove(asset);

           return true;
           
        }
        
        public bool Update(Asset entity, object id = null)
        {
            if (id != null && (entity == null || string.IsNullOrEmpty(id.ToString()))) return false;

            // Mimic a real update - useful for debug/testing.
            var assets = Retreive(a => a.Profile.TickerSymbol == id.ToString().Trim().ToUpper()).AsQueryable();
            var assetToUpdate = assets.First();
            assetToUpdate.Profile.TickerSymbol = entity.Profile.TickerSymbol;
            assetToUpdate.Profile.TickerDescription = entity.Profile.TickerDescription;
            assetToUpdate.AssetClass.Code = entity.AssetClass.Code;
            assetToUpdate.Revenue.First().Actual = entity.Revenue.First().Actual;
            assetToUpdate.Revenue.First().DateRecvd = entity.Revenue.First().DateRecvd;
            
            return true;
        }



    }
}

