// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Asp.Versioning.OData;
using Microsoft.AspNet.OData;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;

public class VersionedMetadataControllerTest
{
    [Fact]
    public async Task options_should_return_expected_headers()
    {
        // arrange
        var configuration = new HttpConfiguration()
        {
            IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
        };
        var builder = new VersionedODataModelBuilder( configuration )
        {
            DefaultModelConfiguration = ( b, v, r ) => b.EntitySet<TestEntity>( "Tests" ),
        };
        using var metadata = new VersionedMetadataController() { Configuration = configuration };
        var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
        var controllerTypes = new List<Type>() { typeof( Controller1 ), typeof( Controller2 ), typeof( VersionedMetadataController ) };
        var resolver = new SimpleDependencyResolver( configuration );

        resolver.AddService(
            typeof( ISunsetPolicyManager ),
            ( sp, t ) => new SunsetPolicyManager( sp.GetRequiredService<HttpConfiguration>().GetApiVersioningOptions() ) );
        configuration.DependencyResolver = resolver;
        configuration.AddApiVersioning(
            options =>
            {
                options.ReportApiVersions = true;
                options.Policies.Sunset( "VersionedMetadata" )
                                .Link( "policies" )
                                    .Title( "Versioning Policy" )
                                    .Type( "text/html" )
                                .Link( "policies/prerelease" )
                                    .Title( "Prereleases" )
                                    .Type( "text/html" );
            } );
        controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

        var models = builder.GetEdmModels();
        var request = new HttpRequestMessage( new HttpMethod( "OPTIONS" ), "http://localhost/$metadata" );

        configuration.MapVersionedODataRoute( "odata", null, models );

        using var server = new HttpServer( configuration );
        using var client = new HttpClient( server );

        // act
        var response = await client.SendAsync( request );

        // assert
        response.EnsureSuccessStatusCode();
        response.Headers.GetValues( "OData-Version" ).Single().Should().Be( "4.0" );
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Beta" );
        response.Headers.GetValues( "Link" ).Should().HaveCount( 2 );
        response.Content.Headers.Allow.Should().BeEquivalentTo( "GET", "OPTIONS" );
    }

    private sealed class SimpleDependencyResolver : ServiceContainer, IDependencyResolver
    {
        public SimpleDependencyResolver( HttpConfiguration configuration ) =>
            AddService( typeof( HttpConfiguration ), configuration );

        public IDependencyScope BeginScope() => this;

        public IEnumerable<object> GetServices( Type serviceType )
        {
            yield return GetService( serviceType );
        }
    }

    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    private sealed class Controller1 : ODataController
    {
        public IHttpActionResult Get() => Ok();
    }

    [ApiVersion( "2.0", Deprecated = true )]
    [ApiVersion( "3.0-Beta", Deprecated = true )]
    [ApiVersion( "3.0" )]
    private sealed class Controller2 : ODataController
    {
        public IHttpActionResult Get() => Ok();
    }
}