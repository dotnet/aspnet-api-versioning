namespace Microsoft.Examples.Controllers
{
    using System.Web.Http;

    [RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
    public class HelloWorldController : ApiController
    {
        // GET api/v{version}/helloworld
        [Route]
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        // GET api/v{version}/helloworld/{id}
        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id = id, version = Request.GetRequestedApiVersion().ToString() } );
    }
}