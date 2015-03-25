using FluentNHibernate.Cfg.Db;
using NHibernate;
using PIMS.Infrastructure.NHibernate.Mappings;
using System.Configuration;


namespace PIMS.IntegrationTest
{
    public class NhDatabaseConfiguration
    {
        public static ISessionFactory CreateSessionFactory()
        {
            // Using test database.

            // TODO: Note use of NHibernate ApplicationUser setup in PIMS.Web.Api.NHibernateConfiguration; replicate here ?
            var testConnString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
            return FluentNHibernate.Cfg.Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008
                .ConnectionString(c => c.Is(testConnString)))
                //.ConnectionString(c => c.Is(@"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True")))
                .CurrentSessionContext("web")
                 // Using mappings from WebApi. 
                 // TODO: Should we use our own /Test/Mappings instead ?
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<IncomeMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ProfileMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PositionMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AssetMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AccountTypeMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<InvestorMap>())
                .BuildSessionFactory();

        }
    }
}
