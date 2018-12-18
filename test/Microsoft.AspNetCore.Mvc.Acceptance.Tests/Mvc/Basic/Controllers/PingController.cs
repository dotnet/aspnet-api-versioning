namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using System;

    [ApiController]
    [ApiVersionNeutral]
    [Route( "api/[controller]" )]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => NoContent();
    }
}