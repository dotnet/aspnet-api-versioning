namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = GetType().Name, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id}", Name = "GetMessageById" )]
        public IActionResult Get( string id ) => Ok( new { Controller = GetType().Name, Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpPost]
        public IActionResult Post() => CreatedAtRoute( "GetMessageById", new { id = 42 }, null );

        [HttpGet( "search" )]
        public IActionResult Search( string query ) => Ok( new { Controller = GetType().Name, Query = query, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}