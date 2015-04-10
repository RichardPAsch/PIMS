using Microsoft.Owin;       // Provides implementation of IAppBuilder
using Owin;                 // Includes IAppBuilder as part of OWIN middleware
// Defines CORS policy
using SharpArch.NHibernate.Web.Mvc;


// Directs Microsoft.Owin as to which Startup class we want to use for our application.
[assembly: OwinStartup(typeof(PIMS.Web.Api.StartUp))]


namespace PIMS.Web.Api
{
    // Other half of 'partial' to deal with Authentication explicitly. 
    public partial class StartUp
    {
        /// <summary>
        ///   OWIN Startup/Entry point. Configuration() gets added as middleware to OWIN pipeline.
        ///   Refer to http://katanaproject.codeplex.com - for details on Katana **
        /// </summary>
        /// <param name="app">
        ///   IAppBuilder - interface designed as a specification of the OWIN application middleware.
        /// </param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureData();
        }


        private static void ConfigureData() {
            var storage = new WebSessionStorage(System.Web.HttpContext.Current.ApplicationInstance);
            DataConfig.Configure(storage);
        }



    }
    
}