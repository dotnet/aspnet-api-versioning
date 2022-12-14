namespace AspVersioning.Examples.Netural.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

/// <summary>
/// Netural API controller hidden from OpenApi UI.
/// </summary>
[ApiVersionNeutral]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Check service is running.
    /// </summary>
    /// <returns>I am up and running.</returns>
    /// <response code="200"></response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult Get()
    {
        return Ok("I am up and running");
    }
}
