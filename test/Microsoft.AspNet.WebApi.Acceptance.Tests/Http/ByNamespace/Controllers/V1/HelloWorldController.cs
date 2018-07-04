namespace Microsoft.Web.Http.ByNamespace.Controllers.V1
{
    using System;
    using System.Web.Http;

    [Obsolete( "Deprecated" )]
    [Route( "api/HelloWorld" )]
    [Route( "api/{version:apiVersion}/HelloWorld" )]
    public class HelloWorldController : ApiController
    {
        public IHttpActionResult Get() => Ok( "V1" );
    }
}