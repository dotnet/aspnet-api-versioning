namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    [ApiVersion( "1.0" )]
    [Route( "api/[controller]" )]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( ValuesController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}