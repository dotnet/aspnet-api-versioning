namespace Microsoft.Web.Http.Basic.Controllers
{
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [Route( "api/values" )]
    public class Values2Controller : ApiController
    {
        public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
    }
}