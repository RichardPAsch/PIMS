using System;
using System.Collections.Generic;
using System.Linq;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryProfileRepository : IGenericRepository<Profile>
    {
        public IQueryable<Profile> RetreiveAll() {

            //var assetProfile = new Profile();
            var listing = new List<Profile>()
                                           {
                                               new Profile
                                               {
                                                   ProfileId = Guid.NewGuid(), // new Guid("a2692700-3e14-4af0-a0d1-4da1fh2204d8"),
                                                   SharePrice = 22.16M,
                                                   DividendFreq = "M",
                                                   DividendYield = 4.892M,
                                                   DividendRate = .912M,
                                                   TickerDescription = "Glaxo Smith Kline",
                                                   EarningsPerShare = 3.46M,
                                                   PE_Ratio = 15.17M,
                                                   LastUpdate = DateTime.Now,
                                                   TickerSymbol = "GSK"
                                               },
                                                new Profile
                                               {
                                                   ProfileId = Guid.NewGuid(),// new Guid("a2622700-3h14-4af0-v0d1-4ra1fh9204t8"),
                                                   SharePrice = 216.06M,
                                                   DividendFreq = "Q",
                                                   DividendYield = 6.71M,
                                                   DividendRate = 1.20M,
                                                   TickerDescription = "International Business Machines",
                                                   EarningsPerShare = 6.71M,
                                                   PE_Ratio = 19.87M,
                                                   LastUpdate = DateTime.Now,
                                                   TickerSymbol = "IBM"
                                               },
                                                new Profile
                                               {
                                                   ProfileId = Guid.NewGuid(), // new Guid("g2492700-3e14-4vf0-a0z3-4da1fh1207j8"),
                                                   SharePrice = 103.01M,
                                                   DividendFreq = "A",
                                                   DividendYield = 6.14M,
                                                   DividendRate = 1.09M,
                                                   TickerDescription = "Honda Motor Co.",
                                                   EarningsPerShare = 7.06M,
                                                   PE_Ratio = 12.91M,
                                                   LastUpdate = DateTime.Now,
                                                   TickerSymbol = "HMC"
                                               }
                                           };

            return listing.AsQueryable();
        }

        public Profile Retreive(object tickerSymbol)
        {
            Profile selectedProfile = null;
            try {
                selectedProfile = RetreiveAll().Single(a => a.TickerSymbol == tickerSymbol.ToString().Trim());
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return null;
            }

            return selectedProfile;
        }

        public Profile RetreiveById(Guid key)
        {
            // all fetches to be via ticker
            return null;
        }

        public bool Create(Profile newEntity)
        {
            var currListing = this.RetreiveAll().ToList();

            // In PROD: check against SQL Server.
            if (currListing.Any(p => p.TickerSymbol.ToUpper().Trim() == newEntity.TickerSymbol.ToUpper().Trim())) return false;
            currListing.Add(newEntity); 
            return true;
        }

        public bool Delete(Guid idGuid)
        {
            var profiles = RetreiveAll();
            try {
                profiles.ToList().Remove(profiles.First(p => p.ProfileId == idGuid));
                return true;
            }
            catch {
                return false;
            }
        }

        public bool Update(Profile entity)
        {
            try {
                // Mimic a real update.
                var profiles = RetreiveAll();
                var item = profiles.First(p => p.TickerSymbol == entity.TickerSymbol);
                item.SharePrice = entity.SharePrice;
            }
            catch (Exception) {
                // Mimic failed update due to some exception.
                return false;
            }


            return true;
        }
    }
}
