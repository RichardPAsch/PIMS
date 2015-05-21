using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAssetRepository : IGenericRepository<Asset>,  IGenericAggregateRepository
    {
        public static string UrlAddress { get; set; }
        private readonly InMemoryInvestorRepository _repoInMemoryInvestorRepository = new InMemoryInvestorRepository();
        private readonly InMemoryProfileRepository _repoInMemoryProfileRepository = new InMemoryProfileRepository();
        private readonly InMemoryPositionRepository _repoInMemoryPositionRepository = new InMemoryPositionRepository();
        private readonly InMemoryIncomeRepository _repoInMemoryIncomeRepository = new InMemoryIncomeRepository();


        string IGenericRepository<Asset>.UrlAddress
        {
            get { return UrlAddress; }
            set { UrlAddress =  value ; }
        }

        // TODO: 1-5-15 -> implement 1. controller, 2. unit tests

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
                                                        LastUpdate = "CB", 
                                                        Description = "Corporate Bond", 
                                                        KeyId = new Guid("75a3983f-da08-4ee6-bfbb-664088133483")
                                                    },
                                    Revenue = _repoInMemoryIncomeRepository.Retreive(i => i.AssetId == new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3")).ToList(),
                                    Positions = _repoInMemoryPositionRepository.Retreive(p => p.InvestorKey == "Asch" && p.Url.Contains("IBM") ).ToList(),
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
                                                        LastUpdate = "CS", 
                                                        Description = "Common Stock", 
                                                        KeyId = new Guid("f63ae3d2-6e5d-425a-8424-ffc3d375eec1")
                                                    },
                                    Revenue = _repoInMemoryIncomeRepository.Retreive(i => i.AssetId ==  new Guid("f9cea918-798b-4323-884f-917090b23858")).ToList(),
                                    Positions = _repoInMemoryPositionRepository.Retreive(p => p.InvestorKey == "Motheral"  && p.Url.Contains("YHO") ).ToList(),
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
                                                        LastUpdate = "MLP", 
                                                        Description = "Master Limited Partnership", 
                                                        KeyId = new Guid("25c33d28-327c-4cbb-858e-1d85224e68c9")
                                                    },
                                    Revenue = _repoInMemoryIncomeRepository.Retreive(i => i.AssetId ==  new Guid("cc950e42-1f08-49b9-880b-a35f4d60b317")).ToList(),
                                    Positions = _repoInMemoryPositionRepository.Retreive(p => p.InvestorKey == "Pinkston"  && p.Url.Contains("ETP") ).ToList(),
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
                                                        LastUpdate = "ETF", 
                                                        Description = "Exchange Traded Fund", 
                                                        KeyId = new Guid("28f8a3d2-42a2-4d39-b503-54a9a3143dcb")
                                                    },
                                    Revenue = _repoInMemoryIncomeRepository.Retreive(i => i.AssetId ==  new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9")).ToList(),
                                    Positions = _repoInMemoryPositionRepository.Retreive(p => p.InvestorKey == "Asch"  && p.Url.Contains("VNR") ).ToList(),
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
                                                        LastUpdate = "CB", 
                                                        Description = "Corporate Bond", 
                                                        KeyId = new Guid("da329599-a301-411d-8167-b8e571a531d1")
                                                    },
                                    Revenue = _repoInMemoryIncomeRepository.Retreive(i => i.AssetId ==  new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa")).ToList(),
                                    Positions = _repoInMemoryPositionRepository.Retreive(p => p.InvestorKey == "Asch"  && p.Url.Contains("AAPL") ).ToList(),
                                    Profile = _repoInMemoryProfileRepository.Retreive(p => p.TickerSymbol == "AAPL").Single()
                                }

                    };

            // Initialize URLs.
            foreach (var item in assetListing)
            {
                item.Url = "http://localhost/Pims.Web.Api/api/Asset/" + item.Profile.TickerSymbol.ToUpper().Trim();
                item.Investor.Url = "http://localhost/Pims.Web.Api/api/Investor/" + item.Investor.FirstName + item.Investor.MiddleInitial + item.Investor.LastName;
                item.AssetClass.Url = "http://localhost/Pims.Web.Api/api/AssetClass/" + item.AssetClass.LastUpdate.Trim().ToUpper();
                item.Profile.Url = "http://localhost/Pims.Web.Api/api/Profile/" + item.Profile.TickerSymbol.Trim().ToUpper();
                
                foreach (var subitem in item.Revenue)
                    subitem.Url = item.Url + "/Income/" + subitem.IncomeId;
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
            var assets = Retreive(a => a.AssetId == (id is Guid ? (Guid) id : new Guid())).AsQueryable();
            var assetToUpdate = assets.First();
            assetToUpdate.Profile.TickerSymbol = entity.Profile.TickerSymbol;
            assetToUpdate.Profile.TickerDescription = entity.Profile.TickerDescription;
            assetToUpdate.AssetClass.LastUpdate = entity.AssetClass.LastUpdate;
            assetToUpdate.Revenue.First().Actual = entity.Revenue.First().Actual;
            assetToUpdate.Revenue.First().DateRecvd = entity.Revenue.First().DateRecvd;
            
            return true;
        }



        //---------------------------------------------------------------------------------------------------------
        // Child aggregate object [Profile, Position, and Income] methods under the control of the aggregate root 
        // (InMemoryAssetRepository -or- AssetRepository), and are NOT dependent on their respective individual
        // repositories for these idempotent actions.
        //---------------------------------------------------------------------------------------------------------

        public bool AggregateCreate<T>(T newEntity, string forInvestor, string forTicker)
        {
            // Case "Profile": - N/A. Creation processed via Yahoo GETs, with persistence handled via aggregate root (AssetRepository) 
            //                   during new asset creation.
            try
            {
                switch (typeof(T).Name)
                {
                    case "Position":
                        {
                            var currentAsset = Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == forTicker.ToUpper().Trim() &&
                                                                                                        a.Investor.LastName.Trim() == forInvestor.Trim())
                                              .AsQueryable();
                            
                            currentAsset.First().Positions.Add(newEntity as Position);
                            break;
                        }
                   case "Income":
                        {
                            var incomeToAdd = newEntity as Income;
                            var currentAsset = Retreive(a => a.AssetId == incomeToAdd.AssetId).AsQueryable();
                            currentAsset.First().Revenue.Add(incomeToAdd);
                            break;
                        }
                }
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        public bool AggregateDelete<T>(Guid keyId)
        {
            try {
                switch (typeof(T).Name)
                {
                    case "Position": 
                    {
                        var existingPosition = RetreiveAll().AsQueryable()
                                                            .SelectMany(a => a.Positions.Where(p => p.PositionId == keyId)).ToList();
                        existingPosition.RemoveAt(0);
                        return true;
                    }
                    case "Profile":
                    {
                        // Mimic Profile removal from a database table.
                        var existingProfile = _repoInMemoryProfileRepository.Retreive(p => p.ProfileId == keyId).AsQueryable().ToList();
                        existingProfile.RemoveAt(0);
                        break;
                    }
                    case "Income":
                    {
                        var existingIncome = _repoInMemoryIncomeRepository.Retreive(i => i.IncomeId == keyId).AsQueryable().ToList();
                        existingIncome.RemoveAt(0);
                        break;
                    }
                }
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public bool AggregateUpdate<T>(T editedEntity, string forInvestor, string forTicker)
        {
            try {
                switch (typeof(T).Name)
                {
                    case "Position":
                    {
                        var recvdPosition = editedEntity as Position;
                        var existingPosition = Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == forTicker.ToUpper().Trim()
                                                          && a.Investor.LastName.Trim() == forInvestor.Trim())
                            .AsQueryable()
                            .SelectMany(a => a.Positions.Where(p => recvdPosition != null && p.Account.AccountTypeDesc.Trim() ==
                                                                    recvdPosition.Account.AccountTypeDesc.Trim()));

                        if (recvdPosition != null)
                        {
                            existingPosition.First().Quantity = recvdPosition.Quantity;
                            existingPosition.First().Account = recvdPosition.Account;
                            existingPosition.First().MarketPrice = recvdPosition.MarketPrice;
                        }
                        break;
                    }
                    case "Profile":
                    {
                        var recvdProfile = editedEntity as Profile;
                        var existingAsset = Retreive(a => a.Profile.TickerSymbol.Trim() == forTicker.ToUpper().Trim())
                                             .AsQueryable().First();
                        // Persistence would go here in PROD.
                        existingAsset.Profile = recvdProfile;
                        break;
                    }
                    case "Income":
                    {
                        var recvdIncome = editedEntity as Income;
                        var matchingAsset = Retreive(a => a.AssetId == recvdIncome.AssetId).AsQueryable().First();
                        // Persistence would go here in PROD.
                        matchingAsset.Revenue.Add(recvdIncome);
                        break;
                    }
                }
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

    }

    
}

