﻿namespace Microsoft.AspNetCore.Mvc.Conventions.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( HelloWorldController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}" )]
        public IActionResult Get( int id ) => Ok( new { Controller = nameof( HelloWorldController ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}