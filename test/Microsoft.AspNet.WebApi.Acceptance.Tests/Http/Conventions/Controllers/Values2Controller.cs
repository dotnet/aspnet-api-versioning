namespace Microsoft.Web.Http.Conventions.Controllers
{
    using System.Web.Http;

    [RoutePrefix( "api/values" )]
    public class Values2Controller : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id, version = Request.GetRequestedApiVersion().ToString() } );

        [Route]
        public IHttpActionResult GetV3() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        [Route( "{id:int}" )]
        public IHttpActionResult GetV3( int id ) => Ok( new { controller = GetType().Name, id, version = Request.GetRequestedApiVersion().ToString() } );
    }
}