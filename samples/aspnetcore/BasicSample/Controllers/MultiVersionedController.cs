namespace Microsoft.Examples.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class MultiVersionedController : ControllerBase
    {
        [HttpGet]
        public string Get( ApiVersion version ) => "Version " + version;

        [HttpGet, MapToApiVersion( "2.0" )]
        public string GetV2( ApiVersion version ) => "Version " + version;
    }
}