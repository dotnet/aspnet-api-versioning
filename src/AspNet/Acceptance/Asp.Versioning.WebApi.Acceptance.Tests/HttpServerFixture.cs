// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Owin.Testing;
using Owin;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Tracing;
using static System.Web.Http.IncludeErrorDetailPolicy;

public abstract partial class HttpServerFixture
{
    protected virtual void OnConfigure( HttpConfiguration configuration ) { }

    private TestServer CreateServer() => TestServer.Create( OnStartup );

    private void OnStartup( IAppBuilder app )
    {
        var configuration = new HttpConfiguration
        {
            IncludeErrorDetailPolicy = Always,
        };

        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), FilteredControllerTypes );
        configuration.Services.Replace( typeof( ITraceWriter ), Debugger.IsAttached ? TraceWriter.Debug : TraceWriter.None );
        configuration.AddApiVersioning( OnAddApiVersioning );
        OnConfigure( configuration );
        app.UseWebApi( configuration );
    }
}