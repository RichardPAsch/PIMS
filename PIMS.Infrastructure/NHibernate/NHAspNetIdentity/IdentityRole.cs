using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using PIMS.Core.Security.Nhibernate.Identity;
using PIMS.Infrastructure.NHibernate.NHAspNetIdentity.DomainModel;


namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity
{
    public class IdentityRole : EntityWithTypedId<string>, IRole
    {
        public virtual string Name { get; set; }

        public virtual ICollection<IdentityUser> Users { get; protected set; }

        public IdentityRole() {
            Users = new List<IdentityUser>();
        }

        public IdentityRole(string roleName) : this() {
            Name = roleName;
        }
    }

    public class IdentityRoleMap : ClassMapping<IdentityRole>
    {
        public IdentityRoleMap() {
            Table("AspNetRoles");
            Id(x => x.Id, m => m.Generator(new UuidHexCombGeneratorDef("D")));
            Property(x => x.Name, m => m.NotNullable(false));
            Bag(x => x.Users, map => {
                map.Table("AspNetUserRoles");
                map.Cascade(Cascade.None);
                map.Key(k => k.Column("RoleId"));
            }, rel => rel.ManyToMany(p => p.Column("UserId")));
        }
    }
}

