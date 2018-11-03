namespace Microsoft.Web.Http.Description.Simulators
{
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [RoutePrefix( "Values" )]
    public class AttributeValues1Controller : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok();
    }
}