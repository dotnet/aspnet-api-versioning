// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1019
#pragma warning disable CA1033
#pragma warning disable CA1813

namespace Asp.Versioning;

using static System.AttributeTargets;

/// <summary>
/// Represents the metadata that describes the <see cref="ApiVersion">version</see>-specific implementation of an API.
/// </summary>
[AttributeUsage( Method, AllowMultiple = true, Inherited = false )]
public class MapToApiVersionAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The <see cref="ApiVersion">API version</see>.</param>
    protected MapToApiVersionAttribute( ApiVersion version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the specified versions.</param>
    /// <param name="version">The API version string.</param>
    protected MapToApiVersionAttribute( IApiVersionParser parser, string version ) : base( parser, version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">A numeric API version.</param>
    public MapToApiVersionAttribute( double version ) : base( version ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapToApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The API version string.</param>
    public MapToApiVersionAttribute( string version ) : base( version ) { }

    ApiVersionProviderOptions IApiVersionProvider.Options => ApiVersionProviderOptions.Mapped;
}