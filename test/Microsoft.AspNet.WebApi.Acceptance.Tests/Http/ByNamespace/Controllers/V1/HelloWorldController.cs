namespace Microsoft.Web.Http.ByNamespace.Controllers.V1
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "1.0", Deprecated = true )]
    [Route( "api/HelloWorld" )]
    [Route( "api/{version:apiVersion}/HelloWorld" )]
    public class HelloWorldController : ApiController
    {
        public IHttpActionResult Get() => Ok( "V1" );
    }
}