// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822
#pragma warning disable IDE0060

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion( "1.0" )]
[ApiVersion( "2.0" )]
[Route( "api/v{version:apiVersion}/values" )]
public class OverlappingRouteTemplateController : ControllerBase
{
    [HttpGet( "{id:int}/{childId}" )]
    public IActionResult Get( int id, string childId ) => Ok( new { id, childId } );

    [HttpGet( "{id:int}/children" )]
    public IActionResult Get( int id ) => Ok( new { id } );

    [HttpGet( "{id:int}/ambiguous" )]
    public IActionResult Ambiguous( int id ) => Ok();

    [HttpGet( "{id:int}/ambiguous" )]
    public IActionResult Ambiguous2( int id ) => Ok();

    [HttpGet( "[action]" )]
    public string Echo() => "Test";

    [HttpGet( "[action]/{id}" )]
    [MapToApiVersion( "1.0" )]
    public string Echo( string id ) => id;
}