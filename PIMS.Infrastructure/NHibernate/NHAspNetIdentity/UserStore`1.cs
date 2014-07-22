using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNet.Identity;
using NHibernate;
using NHibernate.Linq;


/*  Development NOTE:
 *  ALL security-related references should use : PIMS.Core.Security.Nhibernate.Identity... namespace.
 */



namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity
{
    /// <summary>
    /// Customized UserStore w/o dependency on EF6; implements IUserStore using NHibernate,
    /// where TUser is the entity type of the user being stored.
    /// </summary>
    /// <typeparam name="TUser"/>
    public class UserStore<TUser> : IUserLoginStore<TUser>, IUserClaimStore<TUser>, IUserRoleStore<TUser>, 
                                    IUserPasswordStore<TUser>, IUserSecurityStampStore<TUser>, IUserStore<TUser> where TUser : IdentityUser
    {
        private bool _disposed;

        /// <summary>
        /// If true then disposing this object will also dispose (close) the session. 
        /// False means that external code is responsible for disposing the session.
        /// </summary>
        public bool ShouldDisposeSession { get; set; }
        public ISession Context { get; private set; }

        public UserStore(ISession context) {
            if (context == null)
                throw new ArgumentNullException("context");

            ShouldDisposeSession = true;
            Context = context;
        }

        public UserStore()
        {
            //temp fix?
        }

        public virtual Task<TUser> FindByIdAsync(string userId) {
            ThrowIfDisposed();
            return Task.FromResult(Context.Get<TUser>(userId));
        }

        public virtual Task<TUser> FindByNameAsync(string userName) {
            ThrowIfDisposed();
            return Task.FromResult(Context.Query<TUser>().FirstOrDefault(u => String.Equals(u.UserName, userName, StringComparison.CurrentCultureIgnoreCase)));
        }

        public virtual async Task CreateAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                await Task.FromResult(Context.Save(user));
                transaction.Complete();
            }
        }

        public virtual async Task DeleteAsync(TUser user) {
            if (user == null)
                throw new ArgumentNullException("user");
            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                Context.Delete(user);
                transaction.Complete();
                await Task.FromResult(0);
            }
        }

        public virtual async Task UpdateAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                Context.Update(user);
                transaction.Complete();
                await Task.FromResult(0);
            }
        }

        private void ThrowIfDisposed() {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing && Context != null && ShouldDisposeSession)
                Context.Dispose();

            _disposed = true;
            Context = null;
        }

       
        public virtual async Task<TUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null) throw new ArgumentNullException("login");

            var query =
                Context.Query<TUser>()
                    .SelectMany(u => u.Logins, (u, l) => new {u, l})
                    .Where(@t => @t.l.LoginProvider == login.LoginProvider && @t.l.ProviderKey == login.ProviderKey)
                    .Select(@t => @t.u);

            var entity = await Task.FromResult(query.SingleOrDefault());

            return entity;

        }




        public virtual Task AddLoginAsync(TUser user, UserLoginInfo login) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                user.Logins.Add(new IdentityUserLogin {
                    ProviderKey = login.ProviderKey,
                    LoginProvider = login.LoginProvider
                });

                Context.SaveOrUpdate(user);
                transaction.Complete();
            }
            return Task.FromResult(0);
        }

        public virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                var info = user.Logins.SingleOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
                if (info != null) {
                    user.Logins.Remove(info);
                    Context.Update(user);

                }
                transaction.Complete();
            }
            return Task.FromResult(0);
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<UserLoginInfo> result = user.Logins.Select(identityUserLogin => new UserLoginInfo(identityUserLogin.LoginProvider, identityUserLogin.ProviderKey)).ToList();

            return Task.FromResult(result);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(identityUserClaim => new Claim(identityUserClaim.ClaimType, identityUserClaim.ClaimValue)).ToList();

            return Task.FromResult(result);
        }

        public virtual Task AddClaimAsync(TUser user, Claim claim) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                user.Claims.Add(new IdentityUserClaim {
                    User = user,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
                transaction.Complete();
            }

            return Task.FromResult(0);
        }

        public virtual Task RemoveClaimAsync(TUser user, Claim claim) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                foreach (var identityUserClaim in Enumerable.Where(user.Claims, uc => {
                                                                                        if (uc.ClaimValue == claim.Value)
                                                                                            return uc.ClaimType == claim.Type;
                                                                                        return false;
                                                                                      }).ToList()) {
                    user.Claims.Remove(identityUserClaim);
                    Context.Delete(identityUserClaim);
                }
                transaction.Complete();
            }

            return Task.FromResult(0);
        }

        public virtual Task AddToRoleAsync(TUser user, string role) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null, or contain white space.");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                var identityRole = Context.Query<IdentityRole>().SingleOrDefault(r => String.Equals(r.Name, role, StringComparison.CurrentCultureIgnoreCase));
                if (identityRole == null) {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,"Role not found.", new object[] { role }));

                    //throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, new object[] { role }));
                }
                user.Roles.Add(identityRole);
                transaction.Complete();
                return Task.FromResult(0);
            }
        }

        public virtual Task RemoveFromRoleAsync(TUser user, string role) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null, or contain white space.");

            using (var transaction = new TransactionScope(TransactionScopeOption.Required)) {
                var identityUserRole = Enumerable.Where(user.Roles, r => String.Equals(r.Name, role, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (identityUserRole != null) {
                    user.Roles.Remove(identityUserRole);
                    Context.Delete(identityUserRole);
                }
                transaction.Complete();
                return Task.FromResult(0);
            }
        }

        public virtual Task<IList<string>> GetRolesAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult((IList<string>)user.Roles.Select(u => u.Name).ToList());
        }

        public virtual Task<bool> IsInRoleAsync(TUser user, string role) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null, or contain white space.");
            return Task.FromResult(Enumerable.Any(user.Roles, r => r.Name.ToUpper() == role.ToUpper()));
        }

        public virtual Task SetPasswordHashAsync(TUser user, string passwordHash) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task SetSecurityStampAsync(TUser user, string stamp) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(TUser user) {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.SecurityStamp);
        }

        public virtual Task<bool> HasPasswordAsync(TUser user) {
            return Task.FromResult(user.PasswordHash != null);
        }
    }
}
