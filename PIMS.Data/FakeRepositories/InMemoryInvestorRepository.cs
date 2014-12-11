using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryInvestorRepository : IGenericRepository<Investor>
    {

        public static string UrlAddress { get; set; }
        
        public IQueryable<Investor> RetreiveAll()
        {
            var investorListing = new List<Investor>
                               {
                                        new Investor
                                        {
                                            Url = "",
                                            LastName = "Asch", 
                                            FirstName = "Richard", 
                                            Address1 = "544 Shorebird Circle", 
                                            Address2 = "Unit 25102",
                                            City = "Redwood Shores",
                                            ZipCode = "94065",
                                            State = "CA",
                                            Phone = "6505927996",
                                            Mobile = "6504653609",
                                            EMailAddr = "rpasch@rpclassics.net",
                                            BirthDay = new DateTime(1950,10,30).ToString("d"),
                                            MiddleInitial = "P",
                                            AspNetUsersId = Guid.NewGuid(),
                                            DateAdded = new DateTime(2014,09,12,15,22,0).ToString("g")
                                        },
                                        new Investor
                                        {
                                            Url = "",
                                            LastName = "Motheral", 
                                            FirstName = "Patricia", 
                                            Address1 = "544 Shorebird Circle", 
                                            Address2 = "Unit 25102",
                                            City = "Redwood Shores",
                                            ZipCode = "94065",
                                            State = "CA",
                                            Phone = "6505927996",
                                            Mobile = "6504652263",
                                            EMailAddr = "patmot@rpclassics.net",
                                            BirthDay = new DateTime(1957,6,21).ToString("d"),
                                            MiddleInitial = "A",
                                            AspNetUsersId = Guid.NewGuid(),
                                            DateAdded = new DateTime(2014,02,2,11,32,0).ToString("g")
                                        },
                                        new Investor
                                        {
                                            Url = "",
                                            LastName = "Pinkston", 
                                            FirstName = "Olcy", 
                                            Address1 = "1643 Main Street", 
                                            Address2 = "",
                                            City = "Littleton",
                                            ZipCode = "78321",
                                            State = "CO",
                                            Phone = "6505927996",
                                            Mobile = "7028747673",
                                            EMailAddr = "opinkston@comcast.net",
                                            BirthDay = new DateTime(1952,2,20).ToString("d"),
                                            MiddleInitial = "M.",
                                            AspNetUsersId = Guid.NewGuid(),
                                            DateAdded = new DateTime(2013,03,11,15,29,0).ToString("g")
                                        },
                                        new Investor
                                        {
                                            Url = "",
                                            LastName = "Sarles", 
                                            FirstName = "Judy", 
                                            Address1 = "8 Glen Lane", 
                                            Address2 = "",
                                            City = "Novato",
                                            ZipCode = "94945",
                                            State = "CA",
                                            Phone = "",
                                            Mobile = "9259849654",
                                            EMailAddr = "jw94507@yahoo.com",
                                            BirthDay = new DateTime(1963,12,22).ToString("d"),
                                            MiddleInitial = "M",
                                            AspNetUsersId = Guid.NewGuid(),
                                            DateAdded = new DateTime(2014,11,17,14,32,0).ToString("g")
                                        }
                                    };

            return investorListing.AsQueryable();
        }

       
        public IQueryable<Investor> Retreive(Expression<Func<Investor, bool>> predicate, IQueryable<object> data = null)
        {
            return RetreiveAll().Where(predicate);
        }

        public Investor RetreiveById(Guid key) {
            //try {
            //    selectedAsset = RetreiveAll().First(a => a.InvestorId == key);
            //}
            //catch (Exception) {
            //    return null;
            //}

            //return selectedAsset;
            return null;
        }

        public bool Create(Investor newEntity) {
            var currListing = RetreiveAll().ToList();

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

        public bool Update(Investor entity, object id = null)
        {
            //TODO - implement
            //if (id != null && (entity == null || string.IsNullOrEmpty(id.ToString()))) return false;

            //// Mimic a real update - useful for debug/testing.
            //var assets = Retreive(a => a.Profile.TickerSymbol == id.ToString().Trim().ToUpper()).AsQueryable();
            //var assetToUpdate = assets.First();
            //assetToUpdate.Profile.TickerSymbol = entity.Profile.TickerSymbol;
            //assetToUpdate.Profile.TickerDescription = entity.Profile.TickerDescription;
            //assetToUpdate.AssetClass.Code = entity.AssetClass.Code;
            //assetToUpdate.AccountType.AccountTypeDesc = entity.AccountType.AccountTypeDesc;
            //assetToUpdate.Income.Actual = entity.Income.Actual;
            //assetToUpdate.Income.DateRecvd = entity.Income.DateRecvd;

            return true;
        }



        string IGenericRepository<Investor>.UrlAddress {
            get { return UrlAddress; }
            set { UrlAddress = value; }
        }
    }
}


