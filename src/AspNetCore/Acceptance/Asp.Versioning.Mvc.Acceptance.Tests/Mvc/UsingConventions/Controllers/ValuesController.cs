// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingConventions.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new { Controller = nameof( ValuesController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult Get( int id ) => Ok( new { Controller = nameof( ValuesController ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );
}