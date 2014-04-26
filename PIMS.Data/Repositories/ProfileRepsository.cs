using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class ProfileRepository : IGenericRepository<Profile>
    {
        // Profile use case scenarios:
        // 1. POSTed asset with user-entered Profile info
        // 2. POSTed asset with new Profile info from Yahoo.
        // 3. POSTed asset with new Profile info from existing Profile record.
        // 4. GET asset Profile per ticker.
        // 5. PUT asset Profile to update existing (db) Profile from Yahoo.


        public ProfileRepository()
        {
            //throw new NotImplementedException();
        }




        // Not needed.
        public IQueryable<Profile> RetreiveAll()
        {
            return null;
        }

        public Profile Retreive(object property)
        {
            // TODO: Check for EXISTING (persissted) Profile record for asset, if user chooses not to "refresh" profile via Yahoo.Finance
            throw new NotImplementedException();
        }

        public Profile RetreiveById(Guid key)
        {
            // TODO: Retreives EXISTING (persissted) Profile record for asset.
            throw new NotImplementedException();
        }

        public bool Create(Profile newEntity)
        {
            //TODO: Profile data will be from a) user-entered info, or 2) "refresh" profile from Yahoo.Finance
            throw new NotImplementedException();
        }

        // TODO: Accessible via Admin only.
        public bool Delete(Guid idGuid)
        {
            throw new NotImplementedException();
        }


        public bool Update(Profile entity)
        {
            throw new NotImplementedException();
        }
    }
}
