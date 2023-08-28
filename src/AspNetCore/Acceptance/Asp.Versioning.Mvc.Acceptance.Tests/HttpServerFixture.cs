// Copyright (c) .NET Foundation and contributors. All rights reserved.


// Ignore Spelling: app
// Ignore Spelling: Mvc
namespace Asp.Versioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Text;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

public abstract partial class HttpServerFixture
{
    private string visualizationUrl;

    internal string DirectedGraphVisualizationUrl =>
        visualizationUrl ??= GenerateEndpointDirectedGraph( Server.Services );

    protected virtual void OnConfigurePartManager( ApplicationPartManager partManager ) =>
        partManager.ApplicationParts.Add( new TestApplicationPart( FilteredControllerTypes ) );

    protected virtual void OnConfigureServices( IServiceCollection services ) => services.AddProblemDetails().AddControllers();

    protected virtual void OnAddMvcApiVersioning( MvcApiVersioningOptions options ) { }

    protected virtual void OnAddApiVersioning( IApiVersioningBuilder builder ) { }

    protected virtual void OnBuildApplication( IApplicationBuilder app ) =>
        app.UseRouting().UseEndpoints( OnConfigureEndpoints );

    protected virtual void OnConfigureEndpoints( IEndpointRouteBuilder endpoints ) => endpoints.MapControllers();

    private static string GenerateEndpointDirectedGraph( IServiceProvider services )
    {
        const int MaxUriLength = 65519;
        var dfa = services.GetRequiredService<DfaGraphWriter>();
        var dataSource = services.GetRequiredService<EndpointDataSource>();
        string graph;

        using ( var writer = new StringWriter() )
        {
            dfa.Write( dataSource, writer );
            writer.Flush();
            graph = writer.ToString();
        }

        var count = graph.Length / MaxUriLength;
        var fragment = new StringBuilder();

        for ( var i = 0; i <= count; i++ )
        {
            if ( i < count )
            {
                fragment.Append( Uri.EscapeDataString( graph.Substring( MaxUriLength * i, MaxUriLength ) ) );
            }
            else
            {
                fragment.Append( Uri.EscapeDataString( graph[( MaxUriLength * i )..] ) );
            }
        }

        return "https://edotor.net/?engine=dot#" + fragment.ToString();
    }

    private TestServer CreateServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices( OnDefaultConfigureServices )
            .Configure( OnBuildApplication )
            .UseContentRoot( GetContentRoot() );

        return new TestServer( builder );
    }

    private void OnDefaultConfigureServices( IServiceCollection services )
    {
        var partManager = new ApplicationPartManager();

        OnConfigurePartManager( partManager );
        services.Add( Singleton( partManager ) );
        OnConfigureServices( services );

        var builder = services.AddApiVersioning( OnAddApiVersioning )
                              .AddMvc( OnAddMvcApiVersioning );

        OnAddApiVersioning( builder );
    }

    private string GetContentRoot()
    {
        var startupAssembly = GetType().GetTypeInfo().Assembly.GetName().Name;
        var contentRoot = new DirectoryInfo( AppContext.BaseDirectory );

        while ( contentRoot.Name != startupAssembly )
        {
            contentRoot = contentRoot.Parent;
        }

        return contentRoot.FullName;
    }
}