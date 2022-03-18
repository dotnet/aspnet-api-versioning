// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingConventions.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new { Controller = nameof( HelloWorld2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult Get( int id ) => Ok( new { Controller = nameof( HelloWorld2Controller ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet]
    public IActionResult GetV3() => Ok( new { Controller = nameof( HelloWorld2Controller ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult GetV3( int id ) => Ok( new { Controller = nameof( HelloWorld2Controller ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
}