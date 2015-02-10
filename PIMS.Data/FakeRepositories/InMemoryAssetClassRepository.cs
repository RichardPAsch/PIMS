using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAssetClassRepository : IGenericRepository<AssetClass>
    {
        

        public IQueryable<AssetClass> RetreiveAll()
        {
            var listing = new List<AssetClass>
                          {
                                new AssetClass
                                {
                                    KeyId = new Guid("f2695700-3e14-4af0-a0d1-4da1ff2204d8"),
                                    Code = "CS",
                                    Description = "Common Stock"
                                },
                                new AssetClass
                                {
                                    KeyId = new Guid("de85c5dd-8875-4fc1-88ad-be8b506b7678"),
                                    Code = "ETF",
                                    Description = "Exchange Traded Fund"
                                },
                                new AssetClass
                                {
                                    KeyId = new Guid("c5de5be1-86f3-4d3b-8a73-816ece184756"),
                                    Code = "IF",
                                    Description = "Index Fund"
                                },
                                new AssetClass
                                {
                                    KeyId = new Guid("87b3b6a0-caad-4e8e-95ec-8117afa83cb6"),
                                    Code = "MMKT",
                                    Description = "Money Market Fund"
                                },
                                new AssetClass
                                {
                                    KeyId =new Guid("7f54a6a4-edaa-4eb3-bc28-d657cdbd21ec"),
                                    Code = "CB",
                                    Description = "Corporate Bond"
                                },
                                new AssetClass
                                {
                                    KeyId =new Guid("af35d238-66e8-4b56-98fd-39ab547e873f"),
                                    Code = "REIT",
                                    Description = "Real Estate Investment Trust"
                                },
                                new AssetClass
                                {
                                    KeyId =new Guid("f806a9fd-1c84-47b0-896a-7880721a99e1"),
                                    Code = "LP",
                                    Description = "Limited Partnership"
                                },
                                new AssetClass
                                {
                                    KeyId =new Guid("806729ff-454d-46b3-bd62-6ff2c3faac45"),
                                    Code = "SF",
                                    Description = "Stock Fund"
                                },
                                new AssetClass
                                {
                                    KeyId = new Guid("9a6e794a-9455-4468-b915-1e465a05a3ac"),
                                    Code = "PFD",
                                    Description = "Preferred Stock"
                                }
                            };

            return listing.AsQueryable();
        }
        
       
        public IQueryable<AssetClass> Retreive(Expression<Func<AssetClass, bool>> predicate)
        {
            return RetreiveAll().Where(predicate);
        }


        public AssetClass RetreiveById(Guid id)
        {
            AssetClass selectedClass = null;
            try {
                selectedClass = RetreiveAll().Single(a => a.KeyId == id);
            }
            catch (Exception) {
                return null;
            }

            return selectedClass;
        }





        public bool Create(AssetClass newEntity)
        {
            // Deferred until needed!
            //-------------------------
            //var currListing = this.RetreiveAll().ToList();

            //// refactor to single fx - for common use
            //if (currListing.Any(ac => ac.Code.ToUpper().Trim() == newEntity.Code.ToUpper().Trim())) return false;
            //currListing.Add(newEntity); // Save() in PROD
            //return true;

            return false;
        }
        public bool Delete(Guid clGuid)
        {
            // Deferred until needed!
            //-------------------------
            //var classifcations = RetreiveAll();
            //try
            //{
            //    classifcations.ToList().Remove(classifcations.First(c => c.KeyId == clGuid));
            //    return true;
            //}
            //catch(Exception ex)
            //{
            //    var msg = ex.Message;
                return false;
            //}
           
        }
        public bool Update(AssetClass entity,object id)
        {
            // Deferred until needed!
            //-------------------------
            //try
            //{
            //    // Mimic a real update.
            //    var classifcations = RetreiveAll().ToList().Where(ac => ac.Code == entity.Code);
            //    return classifcations.Any();
            //}
            //catch (Exception)
            //{
            //    // Mimic failed update due to some exception.
                return false;
            //}
           
        }
        public string UrlAddress { get; set; }


        
    }
}
