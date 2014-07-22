using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using PIMS.Infrastructure.NHibernate.NHAspNetIdentity.DomainModel;


namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity.Helpers
{
    public static class MappingHelper
    {

        /// <summary>
        /// Gets a mapping that can be used with NHibernate.
        /// </summary>
        /// <param name="additionalTypes">Additional Types that are to be added to the mapping, this is useful for adding your ApplicationUser class</param>
        /// <returns></returns>
        public static HbmMapping GetIdentityMappings(System.Type[] additionalTypes)
        {
            var baseEntityToIgnore = new[]
            { 
                typeof(EntityWithTypedId<int>), 
                typeof(EntityWithTypedId<string>)
            };

            var allEntities = new List<System.Type> { 
                typeof(global::NHibernate.AspNet.Identity.IdentityUser), 
                typeof(global::NHibernate.AspNet.Identity.IdentityRole), 
                typeof(global::NHibernate.AspNet.Identity.IdentityUserLogin), 
                typeof(global::NHibernate.AspNet.Identity.IdentityUserClaim),
            };
            allEntities.AddRange(additionalTypes);

            var mapper = new ConventionModelMapper();
            DefineBaseClass(mapper, baseEntityToIgnore.ToArray());
            mapper.IsComponent((type, declared) => typeof(ValueObject).IsAssignableFrom(type));

            mapper.AddMapping<global::NHibernate.AspNet.Identity.IdentityUserMap>();
            mapper.AddMapping<global::NHibernate.AspNet.Identity.IdentityRoleMap>();
            mapper.AddMapping<global::NHibernate.AspNet.Identity.IdentityUserClaimMap>();

            return mapper.CompileMappingFor(allEntities);
        }

        private static void DefineBaseClass(ConventionModelMapper mapper, System.Type[] baseEntityToIgnore)
        {
            if (baseEntityToIgnore == null) return;
            mapper.IsEntity((type, declared) =>
                baseEntityToIgnore.Any(x => x.IsAssignableFrom(type)) &&
                !baseEntityToIgnore.Any(x => x == type) &&
                !type.IsInterface);
            mapper.IsRootEntity((type, declared) => baseEntityToIgnore.Any(x => x == type.BaseType));
        }

    }
}
