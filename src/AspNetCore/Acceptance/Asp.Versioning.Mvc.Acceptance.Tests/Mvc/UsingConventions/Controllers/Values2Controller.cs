// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingConventions.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/values" )]
public class Values2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get( ApiVersion version ) => Ok( new { Controller = nameof( Values2Controller ), Version = version.ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult Get( int id, ApiVersion version ) => Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = version.ToString() } );

    [HttpGet]
    public IActionResult GetV3( ApiVersion version ) => Ok( new { Controller = nameof( Values2Controller ), Version = version.ToString() } );

    [HttpGet( "{id:int}" )]
    public IActionResult GetV3( int id, ApiVersion version ) => Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = version.ToString() } );
}