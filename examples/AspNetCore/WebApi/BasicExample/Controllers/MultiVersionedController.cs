namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class MultiVersionedController : ControllerBase
{
    // Shared across all versions.
    [HttpGet]
    public string Get( ApiVersion version ) => "Version " + version;

    // [MapToApiVersion] — exact-match. Reachable ONLY for v2.0.
    // Requests under v1.0 or v3.0 receive the configured
    // UnsupportedApiVersionStatusCode (default 400).
    [HttpGet( "legacy" ), MapToApiVersion( 2.0 )]
    public string GetLegacy( ApiVersion version ) => "Legacy " + version;

    // [IntroducedInApiVersion] — "from this version onward against the
    // controller's declared set." Reachable for v2.0 AND v3.0 automatically.
    // Requests under v1.0 receive the per-attribute status (default 404)
    // — distinguishable from "version unknown" (still 400).
    // When v4.0 is added to the controller's [ApiVersion] declarations,
    // this action becomes reachable for v4.0 with no further changes.
    [HttpGet( "modern" ), IntroducedInApiVersion( 2.0 )]
    public string GetModern( ApiVersion version ) => "Modern " + version;
}