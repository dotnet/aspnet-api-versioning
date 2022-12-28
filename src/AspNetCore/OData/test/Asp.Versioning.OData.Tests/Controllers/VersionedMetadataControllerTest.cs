// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Asp.Versioning.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

public class VersionedMetadataControllerTest
{
    [Fact]
    public async Task options_should_return_expected_headers()
    {
        // arrange
        var request = new HttpRequestMessage( new HttpMethod( "OPTIONS" ), "http://localhost/$metadata" );
        var builder = new WebHostBuilder().UseStartup<ODataStartup>();

        using var server = new TestServer( builder );
        using var client = server.CreateClient();

        client.BaseAddress = new Uri( "http://localhost" );

        // act
        var response = ( await client.SendAsync( request ) ).EnsureSuccessStatusCode();

        // assert
        response.Headers.GetValues( "OData-Version" ).Single().Should().Be( "4.0" );
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Beta" );
        response.Content.Headers.Allow.Should().BeEquivalentTo( "GET", "OPTIONS" );
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
                typeof( Tests3Controller ),
                typeof( TestModelConfiguration ) );

            services.AddControllers()
                    .ConfigureApplicationPartManager( m => m.ApplicationParts.Add( testControllers ) )
                    .AddOData();

            services.AddApiVersioning().AddOData();
        }

        public void Configure( IApplicationBuilder app ) =>
            app.UseRouting().UseEndpoints( endpoints => endpoints.MapControllers() );
    }
}