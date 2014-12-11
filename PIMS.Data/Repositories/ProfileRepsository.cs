using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
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


        private readonly ISessionFactory _sfFactory;
        public ProfileRepository(ISessionFactory sfFactory)
        {
            if (sfFactory == null)
                throw new ArgumentNullException("sfFactory");

            _sfFactory = sfFactory;
        }


        public IQueryable<Profile> RetreiveAll() {

            using (var sess = _sfFactory.OpenSession()) {
                var profileQuery = sess.Query<Profile>();
                return profileQuery.ToList().AsQueryable();
            }
        }

        public IQueryable<Profile> Retreive(Expression<Func<Profile, bool>> predicate, IQueryable<object> data = null)
        {
            try {
                return RetreiveAll().Where(predicate);
            }
            catch (Exception) {
                return null;
            }
        }

        public Profile RetreiveById(Guid key)
        {
            var filteredProfile = RetreiveAll().ToList().Where(p => p.AssetId == key);
            return filteredProfile.FirstOrDefault();
        }
        
        public bool Create(Profile newEntity)
        {
            var existingProfiles = this.RetreiveAll().ToList();
            if (existingProfiles.Any(p => p.TickerSymbol.ToUpper().Trim() == newEntity.TickerSymbol.ToUpper().Trim())) return false;

            using (var sess = _sfFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try {
                        sess.Save(newEntity);
                        trx.Commit();
                    }
                    catch(Exception ex)
                    {
                        // TODO: Candidate for logging error.
                        var debugError = ex.Message;
                        return false;
                    }
                }
            }

            return true;
        }

        // TODO: Accessible via Admin only.
        // ReSharper disable once InconsistentNaming
        public bool Delete(Guid ProfileId)
        {
            var deleteOk = true;
            
            // ** To avoid NHibernate.Hql.Ast.ANTLR.QuerySyntaxException], NHibernate needs to load 
            // the OBJECT before deleting it, so that it can cascade deletes through its' object graph. **
            var profileToDelete = RetreiveById(ProfileId);

            using (var sess = _sfFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try
                    {
                        sess.Delete(profileToDelete);
                        trx.Commit();
                    }
                    catch(Exception ex)
                    {
                        // TODO: Candidate for logging.
                        var debugError = ex.Message;
                        deleteOk = false;
                    }
                }
            }

            return deleteOk;
        }
        
        public bool Update(Profile revisedProfile, object id)
        {
            // Do we have an existing record on file to update? If so, update anyway,
            // even if only 1 field is affected.
            var updateOk = true;
            using (var sess = _sfFactory.OpenSession()) {
                using (var trx = sess.BeginTransaction()) {
                    try {
                        sess.Update(revisedProfile);
                        trx.Commit();
                    }
                    catch {
                        updateOk = false;
                    }
                }
            }

            return updateOk;
        }

        public string UrlAddress { get; set; }
    }
}
