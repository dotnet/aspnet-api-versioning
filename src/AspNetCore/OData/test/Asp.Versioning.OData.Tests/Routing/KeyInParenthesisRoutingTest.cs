// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.Controllers;
using Asp.Versioning.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;

// when more than one route component is registered, the service document of a route prefix that begins with a
// non-literal segment; for example, 'api/v{version:apiVersion}', shares a matcher node with the key-in-parenthesis form
// of every entity route of the 'api' prefix because they are all complex segments. the endpoints must remain reachable
public class KeyInParenthesisRoutingTest
{
    [Theory]
    [InlineData( "api/Tests(1)?api-version=1.0" )]
    [InlineData( "api/Tests/1?api-version=1.0" )]
    [InlineData( "api/Tests(1)?api-version=2.0" )]
    [InlineData( "api/v1/Tests(1)" )]
    [InlineData( "api/v1/Tests/1" )]
    [InlineData( "api/v2/Tests(1)" )]
    public async Task get_should_return_200_for_entity( string requestUri )
    {
        // arrange
        using var client = await NewTestClientAsync();

        // act
        var response = await client.GetAsync( requestUri, TestContext.Current.CancellationToken );

        // assert
        response.StatusCode.Should().Be( HttpStatusCode.OK );
    }

    [Theory]
    [InlineData( "api" )]
    [InlineData( "api/$metadata" )]
    public async Task get_should_return_200_for_implicitly_versioned_metadata( string requestUri )
    {
        // arrange
        using var client = await NewTestClientAsync();

        // act
        var response = await client.GetAsync( requestUri, TestContext.Current.CancellationToken );

        // assert
        response.StatusCode.Should().Be( HttpStatusCode.OK );
    }

    // an implicit api version is only meant for the service document and $metadata. an entity route that shares the
    // same matcher node must still report an unspecified api version
    [Theory]
    [InlineData( "api/Tests(1)" )]
    [InlineData( "api/Tests/1" )]
    public async Task get_should_return_400_when_api_version_is_unspecified( string requestUri )
    {
        // arrange
        using var client = await NewTestClientAsync();

        // act
        var response = await client.GetAsync( requestUri, TestContext.Current.CancellationToken );

        // assert
        response.StatusCode.Should().Be( HttpStatusCode.BadRequest );
    }

    private static async Task<HttpClient> NewTestClientAsync()
    {
        var builder = Host.CreateDefaultBuilder()
                          .ConfigureWebHostDefaults( builder => builder.UseTestServer().UseStartup<ODataStartup>() );
        var host = await builder.StartAsync( TestContext.Current.CancellationToken );
        var client = host.GetTestClient();

        client.BaseAddress = new Uri( "http://localhost" );

        return client;
    }

#pragma warning disable IDE0079
#pragma warning disable CA1812
#pragma warning disable CA1822 // Mark members as static

    private sealed class ODataStartup
    {
        public void ConfigureServices( IServiceCollection services )
        {
            var testControllers = new TestApplicationPart(
                typeof( VersionedMetadataController ),
                typeof( TestsController ),
                typeof( Tests2Controller ),
                typeof( TestModelConfiguration ) );

            services.AddControllers()
                    .ConfigureApplicationPartManager( m =>
                    {
                        // the test assembly is added by convention and contains other controllers,
                        // which would otherwise contribute endpoints to the same matcher node
                        m.ApplicationParts.Clear();
                        m.ApplicationParts.Add( testControllers );
                    } )
                    .AddOData( options => options.RouteOptions.EnableKeyAsSegment = true );

            services.AddApiVersioning()
                    .AddOData( options =>
                    {
                        options.AddRouteComponents( "api" );
                        options.AddRouteComponents( "api/v{version:apiVersion}" );
                    } );
        }

        public void Configure( IApplicationBuilder app ) =>
            app.UseRouting().UseEndpoints( endpoints => endpoints.MapControllers() );
    }
}