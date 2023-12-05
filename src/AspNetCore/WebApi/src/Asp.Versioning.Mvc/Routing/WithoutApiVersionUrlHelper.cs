// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

internal sealed class WithoutApiVersionUrlHelper : IUrlHelper
{
    private readonly IUrlHelper decorated;

    public WithoutApiVersionUrlHelper( IUrlHelper decorated ) => this.decorated = decorated;

    public ActionContext ActionContext => decorated.ActionContext;

    private IApiVersioningFeature? Feature => ActionContext.HttpContext.Features.Get<IApiVersioningFeature>();

    public string? Action( UrlActionContext actionContext )
    {
        if ( Feature is IApiVersioningFeature feature )
        {
            using ( new NoApiVersionScope( feature ) )
            {
                return decorated.Action( actionContext );
            }
        }

        return decorated.Action( actionContext );
    }

    [return: NotNullIfNotNull( nameof( contentPath ) )]
    public string? Content( string? contentPath )
    {
        if ( Feature is IApiVersioningFeature feature )
        {
            using ( new NoApiVersionScope( feature ) )
            {
                return decorated.Content( contentPath );
            }
        }

        return decorated.Content( contentPath );
    }

    public bool IsLocalUrl( [NotNullWhen( true )] string? url )
    {
        if ( Feature is IApiVersioningFeature feature )
        {
            using ( new NoApiVersionScope( feature ) )
            {
                return decorated.IsLocalUrl( url );
            }
        }

        return decorated.IsLocalUrl( url );
    }

    public string? Link( string? routeName, object? values )
    {
        if ( Feature is IApiVersioningFeature feature )
        {
            using ( new NoApiVersionScope( feature ) )
            {
                return decorated.Link( routeName, values );
            }
        }

        return decorated.Link( routeName, values );
    }

    public string? RouteUrl( UrlRouteContext routeContext )
    {
        if ( Feature is IApiVersioningFeature feature )
        {
            using ( new NoApiVersionScope( feature ) )
            {
                return decorated.RouteUrl( routeContext );
            }
        }

        return decorated.RouteUrl( routeContext );
    }

    private sealed class NoApiVersionScope : IDisposable
    {
        private readonly IApiVersioningFeature feature;
        private readonly string? rawVersion;
        private readonly ApiVersion? version;
        private bool disposed;

        public NoApiVersionScope( IApiVersioningFeature feature )
        {
            this.feature = feature;
            rawVersion = feature.RawRequestedApiVersion;
            version = feature.RequestedApiVersion;
            feature.RawRequestedApiVersion = default;
            feature.RequestedApiVersion = default;
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            feature.RawRequestedApiVersion = rawVersion;
            feature.RequestedApiVersion = version;
        }
    }
}