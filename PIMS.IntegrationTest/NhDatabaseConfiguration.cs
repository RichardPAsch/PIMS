using FluentNHibernate.Cfg.Db;
using NHibernate;
using PIMS.Infrastructure.NHibernate.Mappings;


namespace PIMS.IntegrationTest
{
    public class NhDatabaseConfiguration
    {
        public static ISessionFactory CreateSessionFactory()
        {
            // Using test database.
            //.MsSql2008.ConnectionString(c => c.Is(connString)))
            return FluentNHibernate.Cfg.Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008
                    .ConnectionString(c => c.Is(@"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True")))
                .CurrentSessionContext("web")
                 // Using mappings from WebApi. 
                 // TODO: Should we use our own /Test/Mappings instead ?
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<IncomeMap>())
                .BuildSessionFactory();

        }
    }
}
