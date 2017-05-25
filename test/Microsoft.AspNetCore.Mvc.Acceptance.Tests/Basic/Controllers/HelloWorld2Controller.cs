namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiVersion( "2.0" )]
    [Route( "api/v{version:apiVersion}/HelloWorld" )]
    public class HelloWorld2Controller : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = GetType().Name, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}", Name = "GetMessageById-V2" )]
        public IActionResult Get( int id ) => Ok( new { Controller = GetType().Name, Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpPost]
        public IActionResult Post() => CreatedAtRoute( "GetMessageById-V2", new { id = 42 }, null );

        [HttpGet( "search" )]
        public IActionResult Search( string query ) => Ok( new { Controller = GetType().Name, Query = query, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}