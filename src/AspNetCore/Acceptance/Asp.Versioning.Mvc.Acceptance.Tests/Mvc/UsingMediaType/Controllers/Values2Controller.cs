// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Mvc.UsingMediaType.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[ApiVersion( "2.0" )]
[Route( "api/values" )]
public class Values2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get( ApiVersion version ) =>
        Ok( new { Controller = nameof( Values2Controller ), Version = version.ToString() } );

    [HttpGet( "{id}" )]
    public IActionResult Get( string id, ApiVersion version ) =>
        Ok( new { Controller = nameof( Values2Controller ), Id = id, Version = version.ToString() } );

    [HttpPost]
    public IActionResult Post( JsonElement json ) => CreatedAtAction( nameof( Get ), new { id = "42" }, json );

    [HttpPatch( "{id}" )]
    [Consumes( "application/merge-patch+json" )]
    public IActionResult MergePatch( string id, JsonElement json ) => NoContent();
}