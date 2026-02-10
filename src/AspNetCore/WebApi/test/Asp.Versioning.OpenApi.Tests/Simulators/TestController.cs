// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822
#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

using Microsoft.AspNetCore.Mvc;

[ApiVersion( 1.0 )]
[ApiController]
[Route( "[controller]" )]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test
    /// </summary>
    /// <description>A test API.</description>
    /// <param name="id">A test parameter.</param>
    /// <returns>The original identifier.</returns>
    /// <response code="200">Pass</response>
    /// <response code="400">Fail</response>
    [HttpGet]
    public int Get( int id ) => id;
}