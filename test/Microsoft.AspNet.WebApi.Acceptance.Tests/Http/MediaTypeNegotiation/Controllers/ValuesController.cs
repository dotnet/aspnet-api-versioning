namespace Microsoft.Web.Http.MediaTypeNegotiation.Controllers
{
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [Route( "api/values" )]
    public class ValuesController : ApiController
    {
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
    }
}