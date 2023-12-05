namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using System.Net.Http;
using System.Web.Http;

[ApiVersion( 2.0 )]
[Route( "api/values" )]
public class Values2Controller : ApiController
{
    // GET api/values?api-version=2.0
    public IHttpActionResult Get() =>
        Ok( new
        {
            controller = GetType().Name,
            version = Request.GetRequestedApiVersion().ToString(),
        } );
}