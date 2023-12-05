// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Represents an API version aware <see cref="LinkGenerator">link generator</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersionLinkGenerator : LinkGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionLinkGenerator"/> class.
    /// </summary>
    /// <param name="linkGenerator">The inner <see cref="LinkGenerator">link generator</see>.</param>
    public ApiVersionLinkGenerator( LinkGenerator linkGenerator ) => LinkGenerator = linkGenerator;

    /// <summary>
    /// Gets the inner link generator.
    /// </summary>
    /// <value>The inner <see cref="LinkGenerator">link generator</see>.</value>
    protected LinkGenerator LinkGenerator { get; }

    /// <inheritdoc />
    public override string? GetPathByAddress<TAddress>(
        HttpContext httpContext,
        TAddress address,
        RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null,
        PathString? pathBase = null,
        FragmentString fragment = default,
        LinkOptions? options = null )
    {
        AddApiVersionRouteValueIfNecessary( httpContext, values );
        return LinkGenerator.GetPathByAddress( httpContext, address, values, ambientValues, pathBase, fragment, options );
    }

    /// <inheritdoc />
    public override string? GetPathByAddress<TAddress>(
        TAddress address,
        RouteValueDictionary values,
        PathString pathBase = default,
        FragmentString fragment = default,
        LinkOptions? options = null ) => LinkGenerator.GetPathByAddress( address, values, pathBase, fragment, options );

    /// <inheritdoc />
    public override string? GetUriByAddress<TAddress>(
        HttpContext httpContext,
        TAddress address,
        RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null,
        string? scheme = null,
        HostString? host = null,
        PathString? pathBase = null,
        FragmentString fragment = default,
        LinkOptions? options = null )
    {
        AddApiVersionRouteValueIfNecessary( httpContext, values );
        return LinkGenerator.GetUriByAddress( httpContext, address, values, ambientValues, scheme, host, pathBase, fragment, options );
    }

    /// <inheritdoc />
    public override string? GetUriByAddress<TAddress>(
        TAddress address,
        RouteValueDictionary values,
        string scheme,
        HostString host,
        PathString pathBase = default,
        FragmentString fragment = default,
        LinkOptions? options = null ) => LinkGenerator.GetUriByAddress( address, values, scheme, host, pathBase, fragment, options );

    private static void AddApiVersionRouteValueIfNecessary( HttpContext httpContext, RouteValueDictionary values )
    {
        ArgumentNullException.ThrowIfNull( httpContext );
        ArgumentNullException.ThrowIfNull( values );

        var feature = httpContext.ApiVersioningFeature();
        var key = feature.RouteParameter;

        if ( string.IsNullOrEmpty( key ) )
        {
            return;
        }

        var value = feature.RawRequestedApiVersion;

        if ( !string.IsNullOrEmpty( value ) && !values.ContainsKey( key ) )
        {
            values.Add( key, value );
        }
    }
}