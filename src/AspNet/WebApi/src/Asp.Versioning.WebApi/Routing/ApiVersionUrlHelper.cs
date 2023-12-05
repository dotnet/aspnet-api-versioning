// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Web.Http.Routing;

/// <summary>
/// Represents an API version aware <see cref="UrlHelper">URL helper</see>.
/// </summary>
public class ApiVersionUrlHelper : UrlHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionUrlHelper"/> class.
    /// </summary>
    /// <param name="url">The inner <see cref="UrlHelper">URL helper</see>.</param>
    public ApiVersionUrlHelper( UrlHelper url )
    {
        Url = url ?? throw new System.ArgumentNullException( nameof( url ) );

        if ( url.Request != null )
        {
            Request = url.Request;
        }
    }

    /// <summary>
    /// Gets the inner URL helper.
    /// </summary>
    /// <value>The inner <see cref="UrlHelper">URL helper</see>.</value>
    protected UrlHelper Url { get; }

    /// <inheritdoc />
    public override string Content( string path ) => Url.Content( path );

    /// <inheritdoc />
    public override string Link( string routeName, object routeValues ) =>
        Url.Link( routeName, AddApiVersionRouteValueIfNecessary( new HttpRouteValueDictionary( routeValues ) ) );

    /// <inheritdoc />
    public override string Link( string routeName, IDictionary<string, object> routeValues ) =>
        Url.Link( routeName, AddApiVersionRouteValueIfNecessary( routeValues ) );

    /// <inheritdoc />
    public override string Route( string routeName, object routeValues ) =>
        Url.Route( routeName, AddApiVersionRouteValueIfNecessary( new HttpRouteValueDictionary( routeValues ) ) );

    /// <inheritdoc />
    public override string Route( string routeName, IDictionary<string, object> routeValues ) =>
        Url.Route( routeName, AddApiVersionRouteValueIfNecessary( routeValues ) );

    private IDictionary<string, object>? AddApiVersionRouteValueIfNecessary( IDictionary<string, object>? routeValues )
    {
        if ( Request == null )
        {
            return routeValues;
        }

        var properties = Request.ApiVersionProperties();
        var key = properties.RouteParameter;

        if ( string.IsNullOrEmpty( key ) )
        {
            return routeValues;
        }

        var value = properties.RawRequestedApiVersion;

        if ( string.IsNullOrEmpty( value ) )
        {
            return routeValues;
        }

        if ( routeValues == null )
        {
            return new HttpRouteValueDictionary() { [key!] = value! };
        }

        if ( !routeValues.ContainsKey( key! ) )
        {
            routeValues.Add( key!, value! );
        }

        return routeValues;
    }
}