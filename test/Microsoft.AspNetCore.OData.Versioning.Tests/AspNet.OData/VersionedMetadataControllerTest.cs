namespace Microsoft.AspNet.OData
{
    using Builder;
    using FluentAssertions;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Simulators;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class VersionedMetadataControllerTest
    {
        [Fact]
        public async Task options_should_return_expected_headers()
        {
            // arrange
            var request = new HttpRequestMessage( new HttpMethod( "OPTIONS" ), "http://localhost/$metadata" );
            var response = default( HttpResponseMessage );
            var hostBuilder = new WebHostBuilder().UseStartup<ODataStartup>();

            using ( var server = new TestServer( hostBuilder ) )
            using ( var client = server.CreateClient() )
            {
                client.BaseAddress = new Uri( "http://localhost" );

                // act
                response = ( await client.SendAsync( request ) ).EnsureSuccessStatusCode();
            }

            // assert
            response.Headers.GetValues( "OData-Version" ).Single().Should().Be( "4.0" );
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Beta" );
            response.Content.Headers.Allow.Should().BeEquivalentTo( "GET", "OPTIONS" );
        }

        sealed class ODataStartup
        {
            public void ConfigureServices( IServiceCollection services )
            {
                var testControllers = new TestApplicationPart(
                    typeof( VersionedMetadataController ),
                    typeof( TestsController ),
                    typeof( TestsController2 ),
                    typeof( TestsController3 ) );

                services.AddMvc( options => options.EnableEndpointRouting = false )
                        .ConfigureApplicationPartManager( m => m.ApplicationParts.Add( testControllers ) );

                services.AddApiVersioning()
                        .AddOData()
                        .EnableApiVersioning();
            }

            public void Configure( IApplicationBuilder app, VersionedODataModelBuilder builder )
            {
                app.UseMvc( r => r.MapVersionedODataRoutes( "odata", null, builder.GetEdmModels() ) );
            }
        }
    }
}