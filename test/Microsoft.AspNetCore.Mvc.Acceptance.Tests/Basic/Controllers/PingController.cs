namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using System;

    [ApiVersionNeutral]
    [Route( "api/[controller]" )]
    public class PingController : Controller
    {
        [HttpGet]
        public IActionResult Get() => NoContent();
    }
}