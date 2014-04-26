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
                                                   ProfileId = new Guid("e07a582a-aec8-43b9-9cb8-faed5e5434de"),
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
            Profile selectedProfile = null;
            try {
                selectedProfile = RetreiveAll().Single(a => a.ProfileId == key);
            }
            catch (Exception ex) {
                var msg = ex.Message; // for debug
                return null;
            }

            return selectedProfile;
        }

        public bool Create(Profile newEntity)
        {
            var currListing = this.RetreiveAll().ToList();

            // In PROD: check against SQL Server for existing record.
            if (currListing.Any(p => p.TickerSymbol.ToUpper().Trim() == newEntity.TickerSymbol.ToUpper().Trim())) return false;
            currListing.Add(newEntity); 
            return true;
        }

        public bool Delete(Guid idGuid)
        {
            // In PROD, the following constraints apply:
            // 1. ADMIN only access,
            // 2. no deletion allowed if any referencing Asset exist

            var profiles = RetreiveAll().ToList();
            var oldCount = profiles.Count();
            try {
                profiles.Remove(profiles.First(p => p.ProfileId == idGuid));
                return oldCount != profiles.Count();
            }
            catch {
                return false;
            }
        }

        public bool Update(Profile updatedProfile)
        {
            var resp = true;
            try {
                // Mimic a real update.
                var profiles = RetreiveAll().ToList();
                var oldProfile = profiles.First(p => p.TickerSymbol == updatedProfile.TickerSymbol);
                profiles.Remove(oldProfile);
                profiles.Add(updatedProfile);
                resp = oldProfile.SharePrice != updatedProfile.SharePrice;
            }
            catch (Exception) {
                // Mimic failed update due to some exception.
                resp = false;
            }
            
            return resp;
        }


    }
}
