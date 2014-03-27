using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using FluentNHibernate.Conventions;
using NHibernate.SqlCommand;
using PIMS.Core.Models;
using PIMS.Data.Repositories;

//TODO Note:
/*
 * DO AWAY with using hand-rolled moq objects, as is used here.
 * CONS: more code = more complexity (more moq code or more complex code)
 *       interface changes make (prod) code more brittle
 *       any interaction logic chamges increases brittleness
 */

namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAssetClassRepository : IGenericRepository<AssetClass>
    {

        public IQueryable<AssetClass> RetreiveAll()
        {
            var assetAssetClass = new AssetClass();
            List<AssetClass> listing = new List<AssetClass>()
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
                                                   KeyId =new Guid("3a943b88-70d0-42ee-b6f1-ce4044dabe86"),
                                                   Code = "MF",
                                                   Description = "Mutual Fund"
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


        //public AssetClass Retreive(Expression<Func<AssetClass, int, bool>> predicate)
        //{
        //    var classification = RetreiveAll().Where(predicate).Single();
        //    return classification;
        //}


        public AssetClass Retreive(object assetCode)
        {
            AssetClass selectedClass = null;
            try
            {
                selectedClass = RetreiveAll().Single(a => a.Code == assetCode.ToString());
            }
            catch(Exception)
            {
                return null;
            }

            return selectedClass;
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
            var currListing = this.RetreiveAll().ToList();

            // refactor to single fx - for common use
            if (currListing.Any(ac => ac.Code.ToUpper().Trim() == newEntity.Code.ToUpper().Trim())) return false;
            currListing.Add(newEntity); // Save() in PROD
            return true;
        }


        public bool Delete(Guid clGuid)
        {
            var classifcations = RetreiveAll();
            try
            {
                classifcations.ToList().Remove(classifcations.First(c => c.KeyId == clGuid));
                return true;
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
           
        }


        public bool Update(AssetClass entity)
        {
            try
            {
                // Mimic a real update.
                var classifcations = RetreiveAll();
                var item = classifcations.First(c => c.Code == entity.Code);
                item.Description = entity.Description;
            }
            catch (Exception)
            {
                // Mimic failed update due to some exception.
                return false;
            }
          
        
            return true;
      
        }
    }
}
