namespace Microsoft.Web.Http.Basic.Controllers
{
    using Microsoft.Web.Http;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [ApiVersionNeutral]
    [RoutePrefix( "api/ping" )]
    public class PingController : ApiController
    {
        [Route]
        public IHttpActionResult Get() => StatusCode( NoContent );
    }
}