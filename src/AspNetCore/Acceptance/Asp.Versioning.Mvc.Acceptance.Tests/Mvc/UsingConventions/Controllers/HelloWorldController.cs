// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingConventions.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new { Controller = nameof( HelloWorldController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult Get( int id ) => Ok( new { Controller = nameof( HelloWorldController ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
}