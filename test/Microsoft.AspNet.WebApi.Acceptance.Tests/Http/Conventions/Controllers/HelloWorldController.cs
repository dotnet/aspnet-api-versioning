namespace Microsoft.Web.Http.Conventions.Controllers
{
    using System.Web.Http;

    [RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id, version = Request.GetRequestedApiVersion().ToString() } );
    }
}