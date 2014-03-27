using System.Web.Mvc;
using System.Web.Routing;


namespace PIMS.Web.Api
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "GetAssetClass",
            //    url: "{controller}/{Code}",
            //    defaults: new { controller = "AssetClass", Code = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}