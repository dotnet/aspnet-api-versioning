namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
    public class HelloWorldController : ApiController
    {
        // GET api/v{version}/helloworld
        [Route]
        public IHttpActionResult Get( ApiVersion apiVersion ) => Ok( new { controller = GetType().Name, version = apiVersion.ToString() } );

        // GET api/v{version}/helloworld/{id}
        [Route( "{id:int}", Name = "GetMessageById" )]
        public IHttpActionResult Get( int id, ApiVersion apiVersion ) => Ok( new { controller = GetType().Name, id, version = apiVersion.ToString() } );

        // POST api/v{version}/helloworld
        [Route]
        public IHttpActionResult Post() => CreatedAtRoute( "GetMessageById", new { id = 42 }, default( object ) );
    }
}