using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Newtonsoft.Json;


namespace PIMS.UnitTest
{
    public static class TestHelpers
    {
        public static string ObjectToJson(object objToSerialize)
        {
            return JsonConvert.SerializeObject(objToSerialize);
        }
       
        public static HttpRequestMessage GetHttpRequestMessage()
        {
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
            return request;
        }


        public static HttpRequestMessage GetHttpRequestMessage(params object[] routeInfo) {

            // Boiler plate setup: mimics controller context state setup that run-time engine expects
            // when handling a POST; this may be unnecessary with WebApi v2.x?

            /*   Indices:
                 * 0 - HttpMethod VERB
                 * 1 - target URL
                 * 2 - controller (Api)
                 * 3 - route name
                 * 4 - route template
                 * 5 - route default(s)
            */


            var request = new HttpRequestMessage( (HttpMethod) routeInfo[0], routeInfo[1].ToString());
            var httpCfg = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpCfg;

            /* Controller */
            var ctrl = (ApiController) routeInfo[2];
            ctrl.Request = request;
            ctrl.Configuration = httpCfg;
            
            /* Route collection*/
            httpCfg.Routes.MapHttpRoute(routeInfo[3].ToString(), routeInfo[4].ToString(), routeInfo[5]);

            return request;
        }
    }
}
