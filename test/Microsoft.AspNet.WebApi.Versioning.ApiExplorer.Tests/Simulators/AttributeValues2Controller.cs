namespace Microsoft.Web.Http.Description.Simulators
{
    using Microsoft.Web.Http.Description.Models;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0-beta", Deprecated = true )]
    [ApiVersion( "3.0" )]
    [RoutePrefix( "values" )]
    public class AttributeValues2Controller : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok();

        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok();

        [Route]
        [MapToApiVersion( "3.0" )]
        public IHttpActionResult Post( ClassWithId resource )
        {
            resource.Id = 1;
            return Created( "values/1", resource );
        }
    }
}