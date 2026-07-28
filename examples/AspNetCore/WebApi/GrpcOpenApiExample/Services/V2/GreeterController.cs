namespace ApiVersioning.Examples.Services.V2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a RESTful service for greeting users
/// </summary>
[ApiController]
[ApiVersion( 2.0 )]
[Produces( "application/json" )]
[Route( "api/[controller]" )]
public class GreeterController : Controller
{
    /// <summary>
    /// Say Hello
    /// </summary>
    /// <description>Says hello to the specified user</description>
    /// <param name="name">The name of the user to greet</param>
    /// <param name="apiVersion">The requested API version</param>
    /// <returns>A user-specific greeting message</returns>
    /// <response code="200">The greeting was successfully generated</response>
    [HttpGet( "{name}" )]
    public IActionResult Get( string name, ApiVersion apiVersion ) => Ok( $"Hello {name} (v{apiVersion})" );
}