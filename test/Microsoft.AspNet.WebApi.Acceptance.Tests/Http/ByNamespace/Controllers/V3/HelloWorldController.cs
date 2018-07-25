namespace Microsoft.Web.Http.ByNamespace.Controllers.V3
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [Route( "api/HelloWorld" )]
    [Route( "api/{version:apiVersion}/HelloWorld" )]
    public class HelloWorldController : ApiController
    {
        public IHttpActionResult Get() => Ok( "V3" );
    }
}