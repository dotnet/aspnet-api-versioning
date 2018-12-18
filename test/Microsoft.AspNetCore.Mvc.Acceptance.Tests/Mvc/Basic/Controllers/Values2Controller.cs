namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
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

        [HttpGet( "{id:int}" )]
        public IActionResult Get( int id ) => Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "search" )]
        public IActionResult Search( string query ) => Ok( new { Controller = nameof( Values2Controller ), Query = query, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}