using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNet.Identity;
using NHibernate;
using NHibernate.AspNet.Identity;
using NHibernate.Linq;



namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity
{
    public class RoleStore<TRole> : IRoleStore<TRole> where TRole : IdentityRole
    {
        private bool _disposed;

        /// <summary>
        /// If true then disposing this object will also dispose (close) the session. False means that external code is responsible for disposing the session.
        /// </summary>
        public bool ShouldDisposeSession { get; set; }

        public ISession Context { get; private set; }

        public RoleStore(ISession context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ShouldDisposeSession = true;
            Context = context;
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            ThrowIfDisposed();
            return Task.FromResult(Context.Get<TRole>(roleId));
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            ThrowIfDisposed();
            return Task.FromResult(Context.Query<TRole>().FirstOrDefault(u => String.Equals(u.Name, roleName, StringComparison.CurrentCultureIgnoreCase)));
        }

        public virtual async Task CreateAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            await Task.FromResult(Context.Save(role));
        }

        public virtual Task DeleteAsync(TRole role)
        {
            throw new NotSupportedException();
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            using (var transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                Context.Update(role);
                transaction.Complete();
                await Task.FromResult(0);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed && ShouldDisposeSession)
                Context.Dispose();
            _disposed = true;
            Context = null;
        }
    }
}
