using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public class ProfileRepository : IGenericRepository<Profile>
    {
        //    Profile use case scenarios:
        // 1. C - Any creation as part of a new asset, will reference Yahoo Finance for the Profile.
        // 2. R - Any Profile fetches will derive first from a) persisted Db, or secondly, b) Yahoo.
        // 3. U - Any updates will fetch the latest data from Yahoo Finance, regardlees of last update for now.
        // 4. D - Only admin role allowed to delete Profiles - WIP secondary to security implementation.


        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }


        public ProfileRepository(ISessionFactory sessFactory)
        {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }
        
        public IQueryable<Profile> RetreiveAll() {

            var profileQuery = (from profile in _nhSession.Query<Profile>() select profile);
            return profileQuery.AsQueryable();
        }
        
        public Profile RetreiveById(Guid key)
        {
            return _nhSession.Get<Profile>(key);
        }

        public IQueryable<Profile> Retreive(Expression<Func<Profile, bool>> predicate)
        {
            try {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception) {
                return null;
            }
        }
        
        public bool Create(Profile newEntity)
        {
            using (var trx = _nhSession.BeginTransaction())
            {
                try {
                    _nhSession.Save(newEntity);
                    trx.Commit();
                }
                catch(Exception ex)
                {
                    // TODO: Candidate for logging error.
                    var debug = ex;
                    return false;
                }
            }

            return true;
        }
        
        public bool Update(Profile revisedProfile, object id)
        {
            // Do we have an existing record on file to update? If so, update anyway,
            // even if only 1 field is affected.
            var updateOk = true;

            // NHibernate will return the following if an established session may already exist:
            // Error: "a different object with the same identifier value was already associated with 
            //        the session: 8f8aff60-3c14-49d6-8132-a86d0110e01c, of entity: PIMS.Core.Models.Profile" 
            _nhSession.Clear();
            
            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Update(revisedProfile);
                    trx.Commit();
                }
                catch(Exception ex)
                {
                    var debug = ex.Data;
                    updateOk = false;
                }
            }

            return updateOk;
        }



        // ** Per Admin only; rarely performed, as used by many referencing Assets. **
        // ReSharper disable once InconsistentNaming
        public bool Delete(Guid ProfileId)
        {
            var deleteOk = true;

            // ** To avoid NHibernate.Hql.Ast.ANTLR.QuerySyntaxException], NHibernate needs to load 
            // the OBJECT before deleting it, so that it can cascade deletes through its' object graph. **
            var profileToDelete = RetreiveById(ProfileId);

            using (var trx = _nhSession.BeginTransaction()) {
                try {
                    _nhSession.Delete(profileToDelete);
                    trx.Commit();
                }
                catch (Exception) {
                    // TODO: Candidate for logging.
                    deleteOk = false;
                }
            }

            return deleteOk;
        }

       
    }
}
