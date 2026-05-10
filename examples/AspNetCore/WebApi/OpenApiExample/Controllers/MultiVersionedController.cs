namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Demonstrates [MapToApiVersion] (exact-match) vs. [IntroducedInApiVersion]
/// (from-this-version-onward) on a single controller declaring multiple versions.
/// </summary>
/// <remarks>
/// <para>
/// Open the Scalar UI per version (1.0, 2.0, 3.0) to see how the two attributes
/// affect the OpenAPI surface differently:
/// </para>
/// <list type="bullet">
///   <item><description>v1.0 — only the shared <c>GET</c> appears.</description></item>
///   <item><description>
///     v2.0 — shared <c>GET</c>, <c>/legacy</c>, and <c>/modern</c> all appear.
///   </description></item>
///   <item><description>
///     v3.0 — shared <c>GET</c> and <c>/modern</c> appear; <c>/legacy</c> is filtered out
///     because <c>[MapToApiVersion(2.0)]</c> is exact-match. <c>/modern</c> is
///     present without any code change because <c>[IntroducedInApiVersion(2.0)]</c>
///     means "from v2.0 onward against the controller's declared set."
///   </description></item>
/// </list>
/// </remarks>
[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[Route( "api/[controller]" )]
public class MultiVersionedController : ControllerBase
{
    /// <summary>
    /// Get the resource (shared across all versions).
    /// </summary>
    /// <param name="version">The requested API version.</param>
    /// <returns>A version-tagged response.</returns>
    /// <response code="200">The resource was retrieved.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( object ), 200 )]
    public IActionResult Get( ApiVersion version ) =>
        Ok( new { version = version.ToString(), shared = true } );

    /// <summary>
    /// A v2-only endpoint declared with <c>[MapToApiVersion(2.0)]</c>.
    /// </summary>
    /// <remarks>
    /// Reachable ONLY for v2.0. v1.0 and v3.0 callers receive the configured
    /// <c>UnsupportedApiVersionStatusCode</c> (default 400). When v3.0 was
    /// added to this controller's <c>[ApiVersion]</c> declarations, this
    /// action did NOT automatically participate; if v3.0 should reach it, the
    /// attribute must be edited to
    /// <c>[MapToApiVersion(2.0, 3.0)]</c>.
    /// </remarks>
    /// <param name="version">The requested API version.</param>
    /// <response code="200">Reached the v2-only endpoint.</response>
    [HttpGet( "legacy" ), MapToApiVersion( 2.0 )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( object ), 200 )]
    public IActionResult GetLegacy( ApiVersion version ) =>
        Ok( new { version = version.ToString(), legacy = true } );

    /// <summary>
    /// An endpoint introduced in v2.0 declared with <c>[IntroducedInApiVersion(2.0)]</c>.
    /// </summary>
    /// <remarks>
    /// Reachable for v2.0 AND v3.0 automatically. v1.0 callers receive the
    /// per-attribute status (default 404), distinguishable from "version
    /// unknown" (still 400). When v4.0 is added to this controller's
    /// <c>[ApiVersion]</c> declarations, this action becomes reachable for
    /// v4.0 with no further changes.
    /// </remarks>
    /// <param name="version">The requested API version.</param>
    /// <response code="200">Reached the v2-onwards endpoint.</response>
    /// <response code="404">The endpoint did not exist in the requested version.</response>
    [HttpGet( "modern" ), IntroducedInApiVersion( 2.0 )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( object ), 200 )]
    [ProducesResponseType( 404 )]
    public IActionResult GetModern( ApiVersion version ) =>
        Ok( new { version = version.ToString(), modern = true } );
}
