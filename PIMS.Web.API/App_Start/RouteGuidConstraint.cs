using System;
using System.Web;
using System.Web.Routing;

namespace PIMS.Web.Api
{
    public class RouteGuidConstraint : IRouteConstraint
    {
        
        // All /{id} segments in routing should conform to Guid format.
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.ContainsKey(parameterName)) {
                string stringValue = values[parameterName] as string;

                if (!string.IsNullOrEmpty(stringValue)) {
                    Guid guidValue;

                    return Guid.TryParse(stringValue, out guidValue) && (guidValue != Guid.Empty);
                }
            }

            return false;

        }
    }
}