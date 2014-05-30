using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAssetRepository : IGenericRepository<Asset>
    {

        public IQueryable<Asset> RetreiveAll() {

            //var assetProfile = new Profile();
            var listing = new List<Asset>()
                                    {
                                        new Asset
                                        {
                                            AssetId = new Guid("e07a582a-aec8-43b9-9cb8-faed5e5434de"), 
                                            AssetClass = new AssetClass() {Code = "AAPL"},
                                            AccountType = "IRA",
                                            Income = new Income(){Actual = 56.11M, DateRecvd = DateTime.UtcNow, IncomeId = Guid.NewGuid(), Projected = 52.56M},
                                            Position = new Position()
                                                        {
                                                            MarketPrice = 44.23M, 
                                                            PositionId = Guid.NewGuid(), 
                                                            PurchaseDate = DateTime.UtcNow.AddDays(-2), 
                                                            Quantity = 100,
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            TotalValue = 923.11M,
                                                            UnitPrice = 142.93M
                                                        },
                                            Profile = new Profile()
                                                        {
                                                            DividendFreq = "Q",
                                                            DividendRate = 1.32M,
                                                            DividendYield = 4.9M,
                                                            EarningsPerShare = 3.21M,
                                                            LastUpdate = DateTime.UtcNow.AddDays(-4),
                                                            PE_Ratio = 15.66M,
                                                            ProfileId = Guid.NewGuid(),
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            SharePrice = 73.11M,
                                                            TickerDescription = "Apple Inc.",
                                                            TickerSymbol = "AAPL" // TODO: duplicate
                                                        }
                                        },
                                            new Asset
                                        {
                                            AssetId = Guid.NewGuid(), 
                                            AssetClass = new AssetClass() {Code = "YHO"},
                                            AccountType = "Roth-IRA",
                                            Income = new Income(){Actual = 5.31M, DateRecvd = DateTime.UtcNow, IncomeId = Guid.NewGuid(), Projected = 62.26M},
                                            Position = new Position()
                                                        {
                                                            MarketPrice = 23.23M, 
                                                            PositionId = Guid.NewGuid(), 
                                                            PurchaseDate = DateTime.UtcNow.AddDays(-2), 
                                                            Quantity = 200,
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            TotalValue = 49.81M,
                                                            UnitPrice = 152.03M
                                                        },
                                            Profile = new Profile()
                                                        {
                                                            DividendFreq = "S",
                                                            DividendRate = 1.94M,
                                                            DividendYield = 3.99M,
                                                            EarningsPerShare = 2.11M,
                                                            LastUpdate = DateTime.UtcNow.AddDays(-3),
                                                            PE_Ratio = 19.96M,
                                                            ProfileId = Guid.NewGuid(),
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            SharePrice = 103.01M,
                                                            TickerDescription = "Yahoo Inc.",
                                                            TickerSymbol = "YHO"
                                                        }
                                        },
                                        new Asset
                                        {
                                            AssetId = Guid.NewGuid(), 
                                            AssetClass = new AssetClass() {Code = "ETP"},
                                            AccountType = "CMA",
                                            Income = new Income(){Actual = 191.09M, DateRecvd = DateTime.UtcNow.AddDays(-6), IncomeId = Guid.NewGuid(), Projected = 198.44M},
                                            Position = new Position()
                                                        {
                                                            MarketPrice = 86.27M, 
                                                            PositionId = Guid.NewGuid(), 
                                                            PurchaseDate = DateTime.UtcNow.AddDays(-8), 
                                                            Quantity = 300,
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            TotalValue = 2874.33M,
                                                            UnitPrice = 192.98M
                                                        },
                                            Profile = new Profile()
                                                        {
                                                            DividendFreq = "S",
                                                            DividendRate = .992M,
                                                            DividendYield = 6.82M,
                                                            EarningsPerShare = 4.91M,
                                                            LastUpdate = DateTime.UtcNow.AddDays(-7),
                                                            PE_Ratio = 25.76M,
                                                            ProfileId = Guid.NewGuid(),
                                                            //Security = new List<Asset>(), // TODO: re-evaluate this
                                                            SharePrice = 93.51M,
                                                            TickerDescription = "Energy Transfer Partners, MLP.",
                                                            TickerSymbol = "ETP"
                                                        }
                                        }
                                    };

            return listing.AsQueryable();
        }

        public Asset Retreive(object property)
        {
            Asset selectedAsset = null;
            try
            {
                selectedAsset = RetreiveAll().First(a => a.Profile.TickerSymbol == property.ToString());
            }
            catch (Exception) {
                return null;
            }

            return selectedAsset;
        }

        


        public Asset RetreiveById(Guid key) {

            Asset selectedAsset = null;
            try
            {
                selectedAsset = RetreiveAll().First(a => a.AssetId == key);
            }
            catch (Exception) {
                return null;
            }

            return selectedAsset;
        }

        public bool Create(Asset newEntity) {
            var currListing = this.RetreiveAll().ToList();

            // In PROD: check against SQL Server.
            if (currListing.Any(p => p.Profile.TickerSymbol.ToUpper().Trim() == newEntity.Profile.TickerSymbol.ToUpper().Trim())) return false;
            currListing.Add(newEntity);
            return true;
        }

        // TODO: implementation pending
        public bool Delete(Guid idGuid) {
            //var profiles = RetreiveAll();
            //try {
            //    profiles.ToList().Remove(profiles.First(p => p.ProfileId == idGuid));
            //    return true;
            //}
            //catch {
            //    return false;
            //}
            return false;
        }

       
        // TODO: implementation pending
        public bool Update(Asset entity, object id) {
            //try {
            //    // Mimic a real update.
            //    var profiles = RetreiveAll();
            //    var item = profiles.First(p => p.TickerSymbol == entity.TickerSymbol);
            //    item.SharePrice = entity.SharePrice;
            //}
            //catch (Exception) {
            //    // Mimic failed update due to some exception.
            //    return false;
            //}


            return true;
        }
    }
}

