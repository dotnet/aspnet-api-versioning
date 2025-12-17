// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822
#pragma warning disable IDE0060

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

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

    [HttpGet]
    [ProducesResponseType( StatusCodes.Status200OK )]
    public IActionResult Get() => Ok();

    [HttpPost]
    [Consumes( MediaTypeNames.Application.Json )]
    [ProducesResponseType( StatusCodes.Status201Created )]
    public IActionResult Post( [FromBody] string body ) => CreatedAtAction( nameof( Get ), body );
}