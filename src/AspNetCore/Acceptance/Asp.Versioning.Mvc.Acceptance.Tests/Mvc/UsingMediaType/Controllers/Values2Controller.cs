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
    public IActionResult Get( ApiVersion version ) => Ok( new { Controller = nameof( Values2Controller ), Version = version.ToString() } );

    [HttpPatch( "{id}" )]
    [Consumes( "application/merge-patch+json" )]
    public IActionResult MergePatch( JsonElement json ) => NoContent();
}