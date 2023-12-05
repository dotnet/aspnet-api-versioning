// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents the API versioning feature.
/// </summary>
[CLSCompliant( false )]
public sealed class ApiVersioningFeature : IApiVersioningFeature
{
    private readonly HttpContext context;
    private IReadOnlyList<string>? rawApiVersions;
    private ApiVersion? apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersioningFeature"/> class.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
    [CLSCompliant( false )]
    public ApiVersioningFeature( HttpContext context ) => this.context = context;

    /// <inheritdoc />
    public string? RouteParameter { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<string> RawRequestedApiVersions
    {
        get
        {
            if ( rawApiVersions is null )
            {
                var reader = context.RequestServices.GetService<IApiVersionReader>() ?? ApiVersionReader.Default;
                rawApiVersions = reader.Read( context.Request );
            }

            return rawApiVersions;
        }
        set => rawApiVersions = value;
    }

    /// <inheritdoc />
    public string? RawRequestedApiVersion
    {
        get
        {
            var values = RawRequestedApiVersions;

            return values.Count switch
            {
                0 => default,
                1 => values[0],
#pragma warning disable IDE0079
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                _ => throw NewAmbiguousApiVersionException( values ),
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
#pragma warning restore IDE0079
            };
        }
        set
        {
            rawApiVersions = string.IsNullOrEmpty( value ) ? default : [value];
        }
    }

    /// <inheritdoc />
    public ApiVersion? RequestedApiVersion
    {
        get
        {
            if ( apiVersion is not null )
            {
                return apiVersion;
            }

            var value = RawRequestedApiVersion;

            if ( string.IsNullOrEmpty( value ) )
            {
                return apiVersion;
            }

            var parser = context.RequestServices.GetService<IApiVersionParser>()
                         ?? ApiVersionParser.Default;

            try
            {
                apiVersion = parser.Parse( value );
            }
            catch ( FormatException )
            {
                apiVersion = default;
            }

            return apiVersion;
        }
        set
        {
            apiVersion = value;

            if ( apiVersion is not null &&
               ( rawApiVersions is null || rawApiVersions.Count == 0 ) )
            {
                rawApiVersions = [apiVersion.ToString()];
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static AmbiguousApiVersionException NewAmbiguousApiVersionException( IReadOnlyList<string> values ) =>
        new(
            string.Format(
                CultureInfo.CurrentCulture,
                Format.MultipleDifferentApiVersionsRequested,
                string.Join( ", ", [.. values], 0, values.Count ) ),
            values );
}