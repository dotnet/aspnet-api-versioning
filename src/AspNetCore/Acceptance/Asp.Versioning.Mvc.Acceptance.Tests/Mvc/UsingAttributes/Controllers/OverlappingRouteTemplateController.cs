// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion( "1.0" )]
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
}