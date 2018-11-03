namespace Microsoft.Web.Http.Description.Simulators
{
    using Microsoft.Web.Http.Description.Models;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [ApiVersion( "4.0" )]
    [AdvertiseApiVersions( "5.0" )]
    [RoutePrefix( "Values" )]
    public class AttributeValues3Controller : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok();

        [Route( "{id:int}" )]
        public IHttpActionResult Get( int id ) => Ok();

        [Route]
        public IHttpActionResult Post( ClassWithId resource )
        {
            resource.Id = 2;
            return Created( "values/2", resource );
        }

        [Route( "{id:int}" )]
        public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
    }
}