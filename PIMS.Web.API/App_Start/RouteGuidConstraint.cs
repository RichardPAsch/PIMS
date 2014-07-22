using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;


namespace PIMS.Web.Api
{

    public class RouteGuidConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection) {
            if (!values.ContainsKey(parameterName)) return false;
            var stringValue = values[parameterName] as string;

            if (string.IsNullOrEmpty(stringValue)) return false;
            Guid guidValue;

            return Guid.TryParse(stringValue, out guidValue) && (guidValue != Guid.Empty);
        }


    }
}