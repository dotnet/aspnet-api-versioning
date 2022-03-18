// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public IActionResult Get( ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Version = apiVersion.ToString() } );

    [HttpGet( "{id}" )]
    public IActionResult Get( string id, ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Id = id, Version = apiVersion.ToString() } );

    [HttpPost]
    public IActionResult Post( ApiVersion apiVersion ) => CreatedAtAction( nameof( Get ), new { id = 42, version = apiVersion.ToString() }, null );

    [HttpGet( "search" )]
    public IActionResult Search( string query, ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Query = query, Version = apiVersion.ToString() } );

    [HttpGet( "[action]" )]
    [MapToApiVersion( "42.0" )]
    public IActionResult Unreachable( ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Version = apiVersion.ToString() } );
}