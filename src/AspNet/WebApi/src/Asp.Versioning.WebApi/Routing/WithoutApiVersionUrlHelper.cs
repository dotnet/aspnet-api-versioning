// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System;
using System.Collections.Generic;
using System.Web.Http.Routing;

internal sealed class WithoutApiVersionUrlHelper : UrlHelper
{
    private readonly UrlHelper decorated;

    public WithoutApiVersionUrlHelper( UrlHelper decorated ) => this.decorated = decorated;

    private ApiVersionRequestProperties Properties => decorated.Request.ApiVersionProperties();

    public override string Content( string path )
    {
        using ( new NoApiVersionScope( Properties ) )
        {
            return decorated.Content( path );
        }
    }

    public override string Link( string routeName, object routeValues )
    {
        using ( new NoApiVersionScope( Properties ) )
        {
            return decorated.Link( routeName, routeValues );
        }
    }

    public override string Link( string routeName, IDictionary<string, object> routeValues )
    {
        using ( new NoApiVersionScope( Properties ) )
        {
            return decorated.Link( routeName, routeValues );
        }
    }

    public override string Route( string routeName, object routeValues )
    {
        using ( new NoApiVersionScope( Properties ) )
        {
            return decorated.Route( routeName, routeValues );
        }
    }

    public override string Route( string routeName, IDictionary<string, object> routeValues )
    {
        using ( new NoApiVersionScope( Properties ) )
        {
            return decorated.Route( routeName, routeValues );
        }
    }

    private sealed class NoApiVersionScope : IDisposable
    {
        private readonly ApiVersionRequestProperties properties;
        private readonly string? rawVersion;
        private readonly ApiVersion? version;
        private bool disposed;

        public NoApiVersionScope( ApiVersionRequestProperties properties )
        {
            this.properties = properties;
            rawVersion = properties.RawRequestedApiVersion;
            version = properties.RequestedApiVersion;
            properties.RawRequestedApiVersion = default;
            properties.RequestedApiVersion = default;
        }

        public void Dispose()
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;
            properties.RawRequestedApiVersion = rawVersion;
            properties.RequestedApiVersion = version;
        }
    }
}