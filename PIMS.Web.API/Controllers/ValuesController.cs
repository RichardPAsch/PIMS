using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Web.Http;
using NHibernate;


namespace PIMS.Web.Api.Controllers
{
    public class ValuesController : ApiController
    {
        private ISession _session;

        public ValuesController(ISession session) {
            _session = session;
        }




        // GET api/values
        public IEnumerable<string> Get() {
            return new string[] { "Richard", "Patricia", "Get() is ok" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            var res = Get();
            return string.Format("Value for {0} is: {1}", id, res.GetEnumerator().Current);
        }

        // POST api/values
        public void Post([FromBody]string value) {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE api/values/5
        public void Delete(int id) {
        }
    }
}