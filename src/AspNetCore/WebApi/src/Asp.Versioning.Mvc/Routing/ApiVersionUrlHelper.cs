// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Represents an API version aware <see cref="IUrlHelper">URL helper</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersionUrlHelper : IUrlHelper
{
    private readonly IApiVersioningFeature feature;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionUrlHelper"/> class.
    /// </summary>
    /// <param name="actionContext">The current <see cref="ActionContext">action context</see>.</param>
    /// <param name="url">The inner <see cref="IUrlHelper">URL helper</see>.</param>
    public ApiVersionUrlHelper( ActionContext actionContext, IUrlHelper url )
    {
        ActionContext = actionContext ?? throw new ArgumentNullException( nameof( actionContext ) );
        Url = url;
        feature = actionContext.HttpContext.ApiVersioningFeature();
    }

    /// <summary>
    /// Gets the inner URL helper.
    /// </summary>
    /// <value>The inner <see cref="IUrlHelper">URL helper</see>.</value>
    protected IUrlHelper Url { get; }

    /// <summary>
    /// Gets the name of the API version route parameter.
    /// </summary>
    /// <value>The API version route parameter name.</value>
    protected string? RouteParameter => feature.RouteParameter;

    /// <summary>
    /// Gets the API version value.
    /// </summary>
    /// <value>The raw API version value.</value>
    protected string? ApiVersion => feature.RawRequestedApiVersion;

    /// <inheritdoc />
    public ActionContext ActionContext { get; }

    /// <inheritdoc />
    public virtual string? Action( UrlActionContext actionContext )
    {
        ArgumentNullException.ThrowIfNull( actionContext );
        actionContext.Values = AddApiVersionRouteValueIfNecessary( actionContext.Values );
        return Url.Action( actionContext );
    }

    /// <inheritdoc />
    public virtual string? Content( string? contentPath ) => Url.Content( contentPath );

    /// <inheritdoc />
    public virtual string? Link( string? routeName, object? values ) =>
        Url.Link( routeName, AddApiVersionRouteValueIfNecessary( values ) );

    /// <inheritdoc />
#pragma warning disable IDE0079
#pragma warning disable CA1054 // URI-like parameters should not be strings
    public virtual bool IsLocalUrl( string? url ) => Url.IsLocalUrl( url );
#pragma warning restore CA1054 // URI-like parameters should not be strings
#pragma warning restore IDE0079

    /// <inheritdoc />
#pragma warning disable IDE0079
#pragma warning disable CA1055 // URI-like return values should not be strings
    public virtual string? RouteUrl( UrlRouteContext routeContext )
#pragma warning restore CA1055 // URI-like return values should not be strings
#pragma warning restore IDE0079
    {
        ArgumentNullException.ThrowIfNull( routeContext );
        routeContext.Values = AddApiVersionRouteValueIfNecessary( routeContext.Values );
        return Url.RouteUrl( routeContext );
    }

    private object? AddApiVersionRouteValueIfNecessary( object? current )
    {
        var key = RouteParameter;

        if ( string.IsNullOrEmpty( key ) )
        {
            return current;
        }

        var value = ApiVersion;

        if ( string.IsNullOrEmpty( value ) )
        {
            return current;
        }

        if ( current is not RouteValueDictionary values )
        {
            values = current == null ? new() : new( current );
        }

        if ( !values.ContainsKey( key ) )
        {
            values.Add( key, value );
        }

        return values;
    }
}