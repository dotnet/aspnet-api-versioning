namespace Microsoft.Web.Http.ByNamespace.Controllers.V2
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [Route( "api/HelloWorld" )]
    [Route( "api/{version:apiVersion}/HelloWorld" )]
    public class HelloWorldController : ApiController
    {
        public IHttpActionResult Get() => Ok( "V2" );
    }
}