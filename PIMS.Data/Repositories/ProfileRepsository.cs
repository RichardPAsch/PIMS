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
        public ProfileRepository()
        {
            //throw new NotImplementedException();
        }



        //public Profile CreateProfile(Profile newProfile)
        //{
        //    throw new NotImplementedException();
        //}

        //public Profile FetchProfile(long id)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool UpdateProfile(long id)
        //{
        //    throw new NotImplementedException();
        //}

        public IQueryable<Profile> RetreiveAll()
        {
            throw new NotImplementedException();
        }

        public Profile Retreive(object property)
        {
            throw new NotImplementedException();
        }

        public Profile RetreiveById(Guid key)
        {
            throw new NotImplementedException();
        }

        public bool Create(Profile newEntity)
        {
            throw new NotImplementedException();
        }

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
