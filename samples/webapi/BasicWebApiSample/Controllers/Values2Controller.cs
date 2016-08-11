namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [Route( "api/values" )]
    public class Values2Controller : ApiController
    {
        // GET api/values?api-version=2.0
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
    }
}