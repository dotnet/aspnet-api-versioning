namespace Microsoft.Web.Http.Description.Simulators
{
    using Microsoft.Web.Http.Description.Models;
    using System.Web.Http;

    [ControllerName( "Values" )]
    public class Values2Controller : ApiController
    {
        public IHttpActionResult Get() => Ok();

        public IHttpActionResult Get( int id ) => Ok();

        public IHttpActionResult Post( ClassWithId resource )
        {
            resource.Id = 1;
            return Created( "values/1", resource );
        }
    }
}