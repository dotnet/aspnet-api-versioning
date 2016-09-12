namespace Microsoft.Examples.Controllers
{
    using System.Web.Http;

    [RoutePrefix( "api/values" )]
    public class ValuesController : ApiController
    {
        // GET api/values?api-version=1.0
        [Route]
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        // GET api/values/{id}?api-version=1.0
        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id = id, version = Request.GetRequestedApiVersion().ToString() } );
    }
}