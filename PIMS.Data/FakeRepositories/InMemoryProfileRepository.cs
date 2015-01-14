using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryProfileRepository : IGenericRepository<Profile>
    {
        public string UrlAddress { get; set; }

        // Retreive() used by Aggregate root repository linking, when fetching child aggregate Profiles.
        public IQueryable<Profile> RetreiveAll()
        {
            var profileListing = new List<Profile>
                          {
                                new Profile
                                {
                                    Url = "", 
                                    AssetId = new Guid("95d955c9-6eb7-4c0c-8622-34dfe0ce5494"), 
                                    DividendFreq = "S",
                                    DividendRate = 1.94M,
                                    DividendYield = 3.99M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 13).ToString("d"),
                                    EarningsPerShare = 2.11M,
                                    LastUpdate = DateTime.UtcNow.AddDays(-3).ToString("g"),
                                    PE_Ratio = 19.96M,
                                    ProfileId = new Guid("1bb5e7bc-2f1e-4c89-b582-1c864b8c1c9b"),
                                    TickerDescription = "Yahoo Inc.",
                                    TickerSymbol = "YHO"
                                },
                                new Profile
                                {
                                    Url = "",
                                    AssetId = new Guid("4617d721-86f0-4c44-93fa-023fcad31ae2"), 
                                    DividendFreq = "Q",
                                    DividendRate = 1.32M,
                                    DividendYield = 4.9M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 19).ToString("d"),
                                    EarningsPerShare = 3.21M,
                                    LastUpdate = DateTime.UtcNow.AddHours(-4).ToString("g"),
                                    PE_Ratio = 15.66M,
                                    ProfileId = new Guid("9e5cced5-76ec-4533-8b89-619e64859415"),
                                    TickerDescription = "International Business Machines.",
                                    TickerSymbol = "IBM"
                                },
                                new Profile
                                {
                                    ProfileId = new Guid("94dbfe06-df47-42f2-a7c1-7bed488a113b"),
                                    AssetId = new Guid("60a2720b-a41c-45a1-a400-07c9bf8640c1"), 
                                    DividendFreq = "A",
                                    DividendYield = 6.14M,
                                    DividendRate = 1.09M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 9).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14).ToString("d"),
                                    TickerDescription = "Honda Motor Co.",
                                    EarningsPerShare = 7.06M,
                                    PE_Ratio = 12.91M,
                                    LastUpdate = DateTime.Now.ToString("g"),
                                    TickerSymbol = "HMC"
                                },
                                new Profile
                                {
                                    Url = "", //UrlAddress + "/ETP/Profile",
                                    DividendFreq = "S",
                                    DividendRate = .992M,
                                    DividendYield = 6.82M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 11).ToString("d"),
                                    EarningsPerShare = 4.91M,
                                    LastUpdate = DateTime.UtcNow.AddDays(-7).ToString("g"),
                                    PE_Ratio = 25.76M,
                                    ProfileId = new Guid("7477e6a1-66fa-45c1-9b70-d160f03519cf"),
                                    TickerDescription = "Energy Transfer Partners, MLP.",
                                    TickerSymbol = "ETP",
                                    AssetId = new Guid("14726d32-3666-4a7b-88ff-ec5c28c04d0a") 
                                },
                                new Profile
                                {
                                    Url = "", 
                                    DividendFreq = "Q",
                                    DividendRate = 1.92M,
                                    DividendYield = 6.69M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28).ToString("d"),
                                    EarningsPerShare = 4.21M,
                                    LastUpdate = DateTime.UtcNow.AddDays(-11).ToString("g"),
                                    PE_Ratio = 5.96M,
                                    ProfileId = new Guid("66e5cd2f-8cec-4453-8920-af91589decd2"),
                                    TickerDescription = "Vanguard National Resources",
                                    TickerSymbol = "VNR",
                                    AssetId = new Guid("be54c339-687c-4576-9cad-522a6f6fe4f4") 
                                },
                                new Profile
                                {
                                    Url = "", 
                                    DividendFreq = "Q",
                                    DividendRate = 1.32M,
                                    DividendYield = 4.9M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 19).ToString("d"),
                                    EarningsPerShare = 3.21M,
                                    LastUpdate = DateTime.UtcNow.AddDays(-4).ToString("g"),
                                    PE_Ratio = 15.66M,
                                    ProfileId = new Guid("e577110b-67bc-4d0c-a367-725369a4b471"),
                                    TickerDescription = "Apple Inc.",
                                    TickerSymbol = "AAPL",
                                    AssetId = new Guid("1968516e-e2b7-42ea-9675-c8c7e1d69b6b") 
                                }
                          };

            // Initialize URLs.
            foreach (var profile in profileListing) {
                profile.Url = "http://localhost/Pims.Web.Api/api/Profile/" + profile.TickerSymbol.ToUpper().Trim();
            }

            return profileListing.AsQueryable();
        }

        public IQueryable<Profile> Retreive(Expression<Func<Profile, bool>> predicate)
        {
            try
            {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public Profile RetreiveById(Guid key)
        {
            return null;
        }


        // The following 3 methods are superceeded by Aggregate functionality located with the
        // aggregate root, e.g., AssetRepository or InMemoryAssetRepository.
        public bool Create(Profile newEntity)
        {
            return false;
        }
        
        public bool Update(Profile updatedProfile, object id = null)
        {
            return false;
        }
        
        public bool Delete(Guid idGuid)
        {
            return false;
        }


    }
}
