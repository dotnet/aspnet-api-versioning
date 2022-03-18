// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersionNeutral]
[Route( "api/[controller]" )]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => NoContent();
}