﻿using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;


namespace PIMS.Web.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config) {

            config.Routes.MapHttpRoute(
             name: "AssetRoute",
             routeTemplate: "api/{controller}/{ticker}",
             defaults: new { id = RouteParameter.Optional }
         );

            // Needed ?
          //  config.Routes.MapHttpRoute(
          //    name: "ProfileRoute",
          //    routeTemplate: "api/Asset/{ticker}/Profile/{ProfileId}",
          //    defaults: new { controller = "Profile", ProfileId = RouteParameter.Optional }
          //);


            config.Routes.MapHttpRoute(
               name: "DefaultApi",
               routeTemplate: "api/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional },
               constraints: new { id = new RouteGuidConstraint() }
           );

            config.Routes.MapHttpRoute(
                name: "AssetClassRoute",
                routeTemplate: "api/{controller}/{Code}",
                defaults: new { Code = RouteParameter.Optional }
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
