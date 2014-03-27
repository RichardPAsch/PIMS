﻿using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Context;
using PIMS.Infrastructure.NHibernate.Mappings;

namespace PIMS.Web.Api
{
    public class NHibernateConfiguration
    {
        
        public static ISessionFactory CreateSessionFactory(string connString = "")
        {

            // Use by default, production backend.
            if (connString == string.Empty)
                connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS';Integrated Security=True";
          

            return FluentNHibernate.Cfg.Fluently.Configure()
                //TODO: Modify to use web.config settings for connection string
                //.Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("Connection-String"))
                .Database(MsSqlConfiguration.MsSql2008
                .ConnectionString(c => c.Is(connString))
                                //.ShowSql() // temp for testing/debugging.
                                //.IsolationLevel(IsolationLevel.RepeatableRead)
                                //.AdoNetBatchSize(10) // Limit batch inserts (etc) 
                         )
                .CurrentSessionContext("web")  // Use current HttpContext in managing NH ISession.
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>()
                                // Export current mappings for debug, if necessary - NOT WORKING
                                //.ExportTo(@"C:\Development\VS2012\IncomeMgmtSystem\src\PIMS.Data\Documentation")  
                         )
                .BuildSessionFactory();
        }


        // Bind, if necessary, the current HttpContext to NHibernate's ISession.
        public static ISession CheckSession(ISessionFactory sf) {
            if (CurrentSessionContext.HasBind(sf))
                return sf.GetCurrentSession();

            var newSession = sf.OpenSession();
            CurrentSessionContext.Bind(newSession);

            return sf.GetCurrentSession();
        }
        
    }



}