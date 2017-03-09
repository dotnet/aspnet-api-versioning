namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiVersion( "2.0" )]
    [Route( "api/values" )]
    public class Values2Controller : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( Values2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}