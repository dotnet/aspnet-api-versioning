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
        public string GetMe() => "Version 1";

        [HttpGet, MapToApiVersion( "2.0" )]
        public string GetMeV2() => "Version 2";
    }
}