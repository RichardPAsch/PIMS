using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using PIMS.Core.Security.Nhibernate.Identity;
using PIMS.Infrastructure.NHibernate.NHAspNetIdentity.DomainModel;


namespace PIMS.Infrastructure.NHibernate.NHAspNetIdentity
{
    public class IdentityUser : EntityWithTypedId<string>, IUser
    {
        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public ICollection<IdentityRole> Roles { get; protected set; }

        public ICollection<IdentityUserClaim> Claims { get; protected set; }

        public ICollection<IdentityUserLogin> Logins { get; protected set; }
        

        public IdentityUser() {
            Roles = new List<IdentityRole>();
            Claims = new List<IdentityUserClaim>();
            Logins = new List<IdentityUserLogin>();
        }

        public IdentityUser(string userName) : this() {
            UserName = userName;
        }
    }



    public class IdentityUserMap : ClassMapping<IdentityUser>
    {
        public IdentityUserMap() {
            Table("AspNetUsers");
            Id(x => x.Id, m => m.Generator(new UuidHexCombGeneratorDef("D")));

            Property(x => x.UserName);
            Property(x => x.PasswordHash);
            Property(x => x.SecurityStamp);

            Bag(x => x.Claims, map => map.Key(k => k.Column("User_Id")), rel => rel.OneToMany());

            Set(x => x.Logins, cam => {
                cam.Table("AspNetUserLogins");
                cam.Key(km => km.Column("UserId"));
                cam.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
            },
            map => map.Component(comp => {
                comp.Property(p => p.LoginProvider);
                comp.Property(p => p.ProviderKey);
            }));

            Bag(x => x.Roles, map => {
                map.Table("AspNetUserRoles");
                map.Key(k => k.Column("UserId"));
            }, rel => rel.ManyToMany(p => p.Column("RoleId")));
        }
    }

}
