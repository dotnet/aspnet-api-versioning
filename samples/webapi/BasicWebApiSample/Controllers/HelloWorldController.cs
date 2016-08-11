namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/helloworld" )]
    public class HelloWorldController : ApiController
    {
        // GET api/v{version}/helloworld
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
    }
}