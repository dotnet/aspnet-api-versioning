namespace Microsoft.Web.Http.MediaTypeNegotiation.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [RoutePrefix( "api/helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

        [Route( "{id:int}", Name = "GetMessageById" )]
        public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id, version = Request.GetRequestedApiVersion().ToString() } );

        [Route]
        public IHttpActionResult Post( Message message ) => CreatedAtRoute( "GetMessageById", new { id = 42 }, message );
    }
}