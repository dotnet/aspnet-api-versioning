// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1033
#pragma warning disable CA1813

namespace Asp.Versioning;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata that describes the <see cref="ApiVersion">versions</see> associated with an API.
/// </summary>
[AttributeUsage( Class | Method, AllowMultiple = true, Inherited = false )]

public class ApiVersionAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
{
    private ApiVersionProviderOptions options = ApiVersionProviderOptions.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    protected ApiVersionAttribute( ApiVersion version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    protected ApiVersionAttribute( IApiVersionParser parser, string version ) : base( parser, version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">A numeric API version.</param>
    /// <param name="status">The status associated with the API version, if any.</param>
    public ApiVersionAttribute( double version, string? status = default ) : base( new ApiVersion( version, status ) ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    public ApiVersionAttribute( string version ) : base( version ) { }

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

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), Deprecated );
}