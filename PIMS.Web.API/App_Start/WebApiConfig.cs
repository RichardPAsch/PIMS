using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;


namespace PIMS.Web.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config) {

            /*  Development notes:
             *  1. Route mapping order is CRITICAL: most specific -> least specific ordering is required.
             *  2. Route mapping mismatch yields HTTP 404 error.
             *  3. Use custom route constraints when "defaults" and "Constraints" involve the same segment.
             *  4. IIS 7.5 - This application runs within "PIMSWebApi" app pool, the latter of which executes under "Richard" account credentials.
            */


            config.Routes.MapHttpRoute(
                name: "ProfileRoute",
                routeTemplate: "api/Asset/{ticker}/{controller}/{profileId}",
                defaults: new { controller = "Profile", profileId = RouteParameter.Optional }
             );

            config.Routes.MapHttpRoute(
                name: "AssetClassesRoute",
                routeTemplate: "api/{controller}",
                defaults: new { controller = "AssetClass" }
           );

            config.Routes.MapHttpRoute(
                name: "AssetClassRoute",
                routeTemplate: "api/{controller}/{assetClassCode}",
                defaults: new { controller = "AssetClass"},
                constraints: new { assetClassCode = "^[a-zA-Z]+$" }
           );

            config.Routes.MapHttpRoute(
                name: "AssetClassRouteById",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { controller = "AssetClass" }
            );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { controller = "AssetClass" },
                constraints: new { id = new RouteGuidConstraint() }
            );




            // Enable returned Json with camel-case formatting, to allow for easier client JS processing, e.g., property names.
            var jsonFormatting = config.Formatters.OfType<JsonMediaTypeFormatter>().FirstOrDefault();
            if (jsonFormatting != null)
                jsonFormatting.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();




            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?href=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();


            
        }
    }
}
