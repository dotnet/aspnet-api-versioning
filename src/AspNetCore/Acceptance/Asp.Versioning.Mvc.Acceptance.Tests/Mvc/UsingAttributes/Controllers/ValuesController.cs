// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/[controller]" )]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new { Controller = nameof( ValuesController ), Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "{id}" )]
    public IActionResult Get( string id ) => Ok( new { Controller = nameof( ValuesController ), Id = id, Version = HttpContext.GetRequestedApiVersion().ToString() } );

    [HttpGet( "search" )]
    public IActionResult Search( string query ) => Ok( new { Controller = nameof( ValuesController ), Query = query, Version = HttpContext.GetRequestedApiVersion().ToString() } );
}