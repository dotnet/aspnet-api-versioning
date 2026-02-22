namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using System.Web.Http;

[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
[Route( "api/v{version:apiVersion}/multiversioned" )]
public class MultiVersionedController : ApiController
{
    [HttpGet]
    public string Get( ApiVersion version ) => "Version " + version;

    [HttpGet, MapToApiVersion( 2.0 )]
    public string GetV2( ApiVersion version ) => "Version " + version;
}