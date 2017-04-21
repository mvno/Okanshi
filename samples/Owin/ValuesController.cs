using System.Collections.Generic;
using System.Web.Http;

namespace Owin {
    public class ValuesController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        public string Get(int id)
        {
            return "value" + id;
        }
    }
}