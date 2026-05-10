// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1033
#pragma warning disable CA1813

namespace Asp.Versioning;

using static System.AttributeTargets;
#if NETSTANDARD
using DateOnly = System.DateTime;
#endif

/// <summary>
/// Represents metadata that describes the <see cref="ApiVersion">API version</see> in which an API action was introduced.
/// </summary>
/// <remarks>
/// The action is mapped to every API version declared by the containing controller that is greater than or equal to
/// the introduced API version. Requests for a controller-declared API version earlier than the introduced API version
/// are rejected using <see cref="StatusCode"/>. Version-neutral controllers and actions ignore introduced API version
/// metadata because version-neutral endpoints are not constrained to declared API versions.
/// </remarks>
/// <example>
/// A controller that declares API versions 1.0, 2.0, and 3.0 can mark an action with
/// <c>[IntroducedInApiVersion( "2.0" )]</c>. The action is mapped to versions 2.0 and 3.0.
/// A request for version 1.0 is rejected using <see cref="StatusCode"/>. Set
/// <see cref="StatusCode"/> to <see cref="UseConfiguredStatusCode"/> (<c>0</c>) to use the globally configured
/// unsupported API version status code instead.
/// </example>
/// <seealso cref="MapToApiVersionAttribute"/>
[AttributeUsage( Method, AllowMultiple = false, Inherited = false )]
public class IntroducedInApiVersionAttribute : ApiVersionsBaseAttribute, IIntroducedInApiVersionProvider
{
    /// <summary>
    /// The default HTTP status code used when a requested API version is earlier than the introduced API version.
    /// </summary>
    public const int DefaultStatusCode = 404;

    /// <summary>
    /// Indicates that the configured unsupported API version status code should be used.
    /// </summary>
    public const int UseConfiguredStatusCode = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    protected IntroducedInApiVersionAttribute( ApiVersion version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    protected IntroducedInApiVersionAttribute( IApiVersionParser parser, string version ) : base( parser, version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">A numeric API version.</param>
    /// <param name="status">The status associated with the API version, if any.</param>
    public IntroducedInApiVersionAttribute( double version, string? status = default )
        : base( new ApiVersion( version, status ) ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The status associated with the API version, if any.</param>
    public IntroducedInApiVersionAttribute( int year, int month, int day, string? status = default )
        : base( new ApiVersion( new DateOnly( year, month, day ), status ) ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    public IntroducedInApiVersionAttribute( string version ) : base( version ) { }

    ApiVersionProviderOptions IApiVersionProvider.Options => ApiVersionProviderOptions.Introduced;

    /// <summary>
    /// Gets or sets the HTTP status code returned when the requested API version is earlier than the introduced API version.
    /// </summary>
    /// <value>The HTTP status code. The default value is <c>404</c>.</value>
    /// <remarks>Set the value to <see cref="UseConfiguredStatusCode"/> to use the configured unsupported API version status code.</remarks>
    public int StatusCode { get; set; } = DefaultStatusCode;

    /// <inheritdoc />
    public override bool Equals( object? obj ) =>
        obj is IntroducedInApiVersionAttribute other &&
        GetType() == obj.GetType() &&
        base.Equals( obj ) &&
        StatusCode == other.StatusCode;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), StatusCode );
}