using ValueObject = PIMS.Infrastructure.NHibernate.NHAspNetIdentity.DomainModel.ValueObject;


namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity
{
    public class IdentityUserLogin : ValueObject
    {
        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

    }
}
