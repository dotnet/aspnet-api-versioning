namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( HelloWorldController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}", Name = "GetMessageById" )]
        public IActionResult Get( int id ) => Ok( new { Controller = GetType().Name, Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpPost]
        public IActionResult Post() => CreatedAtRoute( "GetMessageById", new { id = 42 }, null );
    }
}