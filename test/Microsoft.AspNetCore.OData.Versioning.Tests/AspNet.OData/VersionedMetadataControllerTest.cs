namespace Microsoft.AspNet.OData
{
    using Builder;
    using FluentAssertions;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Simulators;
    using Moq;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    public class VersionedMetadataControllerTest
    {
        [Fact]
        public async Task options_should_return_expected_headers()
        {
            // arrange
            var apiVersionProvider = new Mock<IODataApiVersionProvider>();
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
            var deprecated = new[] { new ApiVersion( 3, 0, "Beta" ) };
            var request = new HttpRequestMessage( new HttpMethod( "OPTIONS" ), "http://localhost/$metadata" );
            var response = default( HttpResponseMessage );

            apiVersionProvider.SetupGet( p => p.SupportedApiVersions ).Returns( supported );
            apiVersionProvider.SetupGet( p => p.DeprecatedApiVersions ).Returns( deprecated );

            var hostBuilder = new WebHostBuilder().ConfigureServices( s => s.Replace( Singleton( apiVersionProvider.Object ) ) )
                                                  .UseStartup<ODataStartup>();

            using ( var server = new TestServer( hostBuilder ) )
            using ( var client = server.CreateClient() )
            {
                client.BaseAddress = new Uri( "http://localhost" );

                // act
                response = await client.SendAsync( request );
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

                services.AddMvc()
                        .ConfigureApplicationPartManager( m => m.ApplicationParts.Add( testControllers ) );

                services.AddApiVersioning()
                        .AddOData()
                        .EnableApiVersioning();
            }

            public void Configure( IApplicationBuilder app, VersionedODataModelBuilder builder )
            {
                builder.DefaultModelConfiguration = ( b, v ) => b.EntitySet<TestEntity>( "Tests" );
                app.UseMvc( r => r.MapVersionedODataRoutes( "odata", null, builder.GetEdmModels() ) );
            }
        }
    }
}