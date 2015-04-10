using FluentNHibernate.Cfg.Db;
using NHibernate;
using PIMS.Infrastructure.NHibernate.Mappings;


namespace PIMS.IntegrationTest
{
    public class NhDatabaseConfiguration
    {
        public static ISessionFactory CreateSessionFactory()
        {

            // TODO: Note use of NHibernate ApplicationUser setup in PIMS.Web.Api.NHibernateConfiguration; replicate here ?
            // TODO: can't read from Web.config ??
            //var testConnString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;

            return FluentNHibernate.Cfg.Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008
                //.ConnectionString(c => c.Is(testConnString)))
                .ConnectionString(c => c.Is(@"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True")))
                .CurrentSessionContext("web")

                 // PIMS.Infrastructure mappings.
                 //TODO: Needed ?
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<IncomeMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ProfileMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PositionMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AssetMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AccountTypeMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<InvestorMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AssetClassMap>())
                .BuildSessionFactory();

        }
    }
}
