using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NHibernate;
using PIMS.Web.Api.App_Start;
using StructureMap;


namespace PIMS.Web.Api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?href=9394801


    public class WebApiApplication : System.Web.HttpApplication
    {
        // Per application start, we'll only call creation of the session factory once.
        // ReSharper disable once InconsistentNaming
        //private static readonly Lazy<ISessionFactory> _NhSessionFactory = new Lazy<ISessionFactory>(() => NHibernateConfiguration.CreateSessionFactory());

        //public static Lazy<ISessionFactory> NhSessionFactory
        //{
        //    get { return _NhSessionFactory; }
        //}



        protected void Application_Start() {

            // 9/25/13; 'AREAS' not needed -RPA
            //AreaRegistration.RegisterAllAreas(); calls HelpPageRegistration.RegisterArea() - all AreaRegistration derived classes.
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        protected void Application_EndRequest() {
            //  Dispose of all NHibernate sessions, if created on this web request.
            ObjectFactory.ReleaseAndDisposeAllHttpScopedObjects();
        }



    }



}