using System.Web;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Context;
using PIMS.Core.Models;
using PIMS.Infrastructure.NHibernate.Mappings;
using NHibernate.AspNet.Identity.Helpers;


namespace PIMS.Web.Api
{
    public class NHibernateConfiguration
    {
        private const string CurrentSessionKey = "nhibernate.current_session";  // added 3-31-15
        private static string _currentConnString = string.Empty; // added 10.19.17

        
        public static ISessionFactory CreateSessionFactory(string connString = "")
        {

            // Use by default, production backend.
            if (connString == string.Empty)
                connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS';Integrated Security=True";

            _currentConnString = connString;
            // Use default subclassed Identity model.
            var appUsers = new[] { typeof(ApplicationUser) };

            return FluentNHibernate.Cfg.Fluently.Configure()
                //TODO: Modify to use web.config settings for connection string
                //.Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("Connection-String"))
                .ExposeConfiguration(cfg => cfg.AddDeserializedMapping(MappingHelper.GetIdentityMappings(appUsers), null))
                .Database(MsSqlConfiguration.MsSql2008
                                            .ConnectionString(c => c.Is(connString))
                                                    //.ShowSql() // temp for testing/debugging.
                                                    //.IsolationLevel(IsolationLevel.RepeatableRead)
                                                    //.AdoNetBatchSize(10) // Limit batch inserts (etc) 
                         )
                .CurrentSessionContext("web")  // Use current HttpContext in managing NH ISession.
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<AssetMap>()
                                // Export current mappings for debug, if necessary - NOT WORKING
                                //.ExportTo(@"C:\Development\VS2012\IncomeMgmtSystem\src\PIMS.Data\Documentation")  
                         )
                .BuildSessionFactory();
        }


        // Bind, if necessary, the current HttpContext to NHibernate's ISession.
        public static ISession CheckSession(ISessionFactory sf)
        {
            if (CurrentSessionContext.HasBind(sf))
                return sf.GetCurrentSession();

            var newSession = sf.OpenSession();
            CurrentSessionContext.Bind(newSession);

            return sf.GetCurrentSession();
        }


        // added 3-31-15
        public static ISession GetCurrentSession()
        {
            var context = HttpContext.Current;
            var currentSession = context.Items[CurrentSessionKey] as ISession;

            if (currentSession != null) return currentSession;
            currentSession = CreateSessionFactory("").OpenSession(); // sessionFactory.OpenSession();
            context.Items[CurrentSessionKey] = currentSession;

            return currentSession;
        }

        // added 10.19.17
        public static string GetConnectionString()
        {
            return _currentConnString;
        }




    }



}