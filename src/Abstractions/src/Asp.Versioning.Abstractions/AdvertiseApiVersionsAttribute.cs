// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1033
#pragma warning disable CA1813

namespace Asp.Versioning;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata that describes the advertised <see cref="ApiVersion">API versions</see>.
/// </summary>
/// <remarks>Advertised API versions indicate the existence of other versioned API, but the implementation of those
/// APIs are implemented elsewhere.</remarks>
[AttributeUsage( Class | Method, AllowMultiple = true, Inherited = false )]
public class AdvertiseApiVersionsAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
{
    private ApiVersionProviderOptions options = ApiVersionProviderOptions.Advertised;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    protected AdvertiseApiVersionsAttribute( ApiVersion version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    /// <param name="otherVersions">An array of other <see cref="ApiVersion">API versions</see>.</param>
    protected AdvertiseApiVersionsAttribute( ApiVersion version, params ApiVersion[] otherVersions )
        : base( version, otherVersions ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    protected AdvertiseApiVersionsAttribute( IApiVersionParser parser, string version ) : base( parser, version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    /// <param name="otherVersions">An array of other API version strings.</param>
    protected AdvertiseApiVersionsAttribute( IApiVersionParser parser, string version, params string[] otherVersions )
        : base( parser, version, otherVersions ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="version">The numeric API versions.</param>
    /// <param name="otherVersions">An array of other numeric API versions.</param>
    [CLSCompliant( false )]
    public AdvertiseApiVersionsAttribute( double version, params double[] otherVersions )
        : base( version, otherVersions ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    public AdvertiseApiVersionsAttribute( string version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvertiseApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    /// <param name="otherVersions">An array of other API version strings.</param>
    [CLSCompliant( false )]
    public AdvertiseApiVersionsAttribute( string version, params string[] otherVersions )
        : base( version, otherVersions ) { }

    ApiVersionProviderOptions IApiVersionProvider.Options => options;

    /// <summary>
    /// Gets or sets a value indicating whether the specified set of API versions are deprecated.
    /// </summary>
    /// <value>True if the specified set of API versions are deprecated; otherwise, false.
    /// The default value is <c>false</c>.</value>
    public bool Deprecated
    {
        get => ( options & ApiVersionProviderOptions.Deprecated ) == ApiVersionProviderOptions.Deprecated;
        set
        {
            if ( value )
            {
                options |= ApiVersionProviderOptions.Deprecated;
            }
            else
            {
                options &= ~ApiVersionProviderOptions.Deprecated;
            }
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), Deprecated );
}