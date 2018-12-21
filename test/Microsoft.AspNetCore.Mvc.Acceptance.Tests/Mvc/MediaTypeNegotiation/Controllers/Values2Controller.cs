namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    [ApiVersion( "2.0" )]
    [Route( "api/values" )]
    public class Values2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( Values2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}