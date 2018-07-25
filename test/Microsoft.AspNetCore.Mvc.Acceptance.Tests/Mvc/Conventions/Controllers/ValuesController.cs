﻿namespace Microsoft.AspNetCore.Mvc.Conventions.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [Route( "api/[controller]" )]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok( new { Controller = nameof( ValuesController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

        [HttpGet( "{id:int}" )]
        public IActionResult Get( int id ) => Ok( new { Controller = nameof( ValuesController ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
    }
}