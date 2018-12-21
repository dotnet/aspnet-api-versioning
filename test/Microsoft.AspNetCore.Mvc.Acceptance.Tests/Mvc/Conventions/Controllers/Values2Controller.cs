namespace Microsoft.AspNetCore.Mvc.Conventions.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    [Route( "api/values" )]
    public class Values2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( Values2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}" )]
        public IActionResult Get( int id ) => Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet]
        public IActionResult GetV3() => Ok( new { Controller = nameof( Values2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}" )]
        public IActionResult GetV3( int id ) => Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}