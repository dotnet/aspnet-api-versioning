namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.OData;
    using Microsoft.Simulators;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Xunit;
    using static Microsoft.AspNetCore.Mvc.ApiVersion;
    using static Microsoft.AspNetCore.Routing.RouteDirection;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
    using static System.UriComponents;
    using static System.UriFormat;

    public class VersionedODataPathRouteConstraintTest
    {
        [Fact]
        public void match_should_always_return_true_for_uri_resolution()
        {
            // arrange
            var url = new Uri( "http://localhost" );
            var httpContext = NewHttpContext( url, "1.0" );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary();
            var constraint = new VersionedODataPathRouteConstraint( "odata", Default );

            // act
            var result = constraint.Match( httpContext, route, routeKey, values, UrlGeneration );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "2.0" )]
        [InlineData( "3.0" )]
        public void match_should_be_true_when_api_version_is_requested_in_query_string( string apiVersionValue )
        {
            // arrange
            var url = new Uri( "http://localhost/Tests(1)?api-version=" + apiVersionValue );
            var apiVersion = Parse( apiVersionValue );
            var context = NewHttpContext( url, apiVersionValue );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { ["odataPath"] = "Tests(1)" };
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "http://localhost", null, "1.0", true )]
        [InlineData( "http://localhost", null, "2.0", false )]
        [InlineData( "http://localhost/$metadata", "$metadata", "1.0", true )]
        [InlineData( "http://localhost/$metadata", "$metadata", "2.0", false )]
        public void match_should_return_expected_result_for_service_and_metadata_document( string requestUri, string odataPath, string apiVersionValue, bool expected )
        {
            // arrange
            const string rawApiVersion = default;
            var url = new Uri( requestUri );
            var apiVersion = Parse( apiVersionValue );
            var context = NewHttpContext( url, rawApiVersion );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { [nameof( odataPath )] = odataPath };
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( "http://localhost/", null, false )]
        [InlineData( "http://localhost/$metadata", "$metadata", false )]
        [InlineData( "http://localhost/Tests(1)", "Tests(1)", true )]
        public void match_should_return_expected_result_when_controller_is_implicitly_versioned( string requestUri, string odataPath, bool allowImplicitVersioning )
        {
            // arrange
            const string rawApiVersion = default;
            var apiVersion = new ApiVersion( 2, 0 );

            void OnConfigure( ApiVersioningOptions options )
            {
                options.DefaultApiVersion = apiVersion;
                options.AssumeDefaultVersionWhenUnspecified = allowImplicitVersioning;
            }

            var url = new Uri( requestUri );
            var context = NewHttpContext( url, rawApiVersion, configure: OnConfigure );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { [nameof( odataPath )] = odataPath };
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "Tests(1)", true )]
        [InlineData( "NonExistent(1)", false )]
        public void match_should_return_expected_result_when_requested_api_version_is_ambiguous( string odataPath, bool expected )
        {
            // arrange
            var url = new Uri( $"http://localhost/{odataPath}?api-version=1.0&api-version=2.0" );
            var apiVersion = new ApiVersion( 1, 0 );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { [nameof( odataPath )] = odataPath };
            var context = NewHttpContext( url, "1.0" );
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().Be( expected );
        }

        static HttpContext NewHttpContext( Uri url, string rawApiVersion, string routePrefix = null, Action<ApiVersioningOptions> configure = null )
        {

            var features = new Mock<IFeatureCollection>();
            var odataFeature = Mock.Of<IODataFeature>();
            var apiVersioningFeature = Mock.Of<IApiVersioningFeature>();
            var query = new Mock<IQueryCollection>();
            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var services = new ServiceCollection();
            var queryValues = new Dictionary<string, StringValues>( StringComparer.OrdinalIgnoreCase );

            if ( !string.IsNullOrEmpty( url.Query ) )
            {
                foreach ( var values in from item in url.Query.TrimStart( '?' ).Split( '&' )
                                        let parts = item.Split( '=' )
                                        group parts[1] by parts[0] )
                {
                    queryValues.Add( values.Key, new StringValues( values.ToArray() ) );
                }
            }

            services.AddLogging();
            services.Add( Singleton( new DiagnosticListener( "test" ) ) );
            services.AddMvcCore( options => options.EnableEndpointRouting = false )
                    .ConfigureApplicationPartManager( apm => apm.ApplicationParts.Add( new TestApplicationPart( typeof( TestsController ) ) ) );
            services.AddApiVersioning( configure ?? ( _ => { } ) );
            services.AddOData().EnableApiVersioning();

            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder( serviceProvider );
            var modelBuilder = serviceProvider.GetRequiredService<VersionedODataModelBuilder>();

            modelBuilder.ModelConfigurations.Add( new TestModelConfiguration() );

            var model = modelBuilder.GetEdmModels().Single();

            if ( !TryParse( rawApiVersion, out var apiVersion ) )
            {
                apiVersion = serviceProvider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.DefaultApiVersion;
            }

            app.UseMvc( rb => rb.MapVersionedODataRoute( "odata", routePrefix, model, apiVersion ) );
            apiVersioningFeature.RawRequestedApiVersion = rawApiVersion;
            apiVersioningFeature.RequestedApiVersion = apiVersion;
            features.SetupGet( f => f[typeof( IODataFeature )] ).Returns( odataFeature );
            features.Setup( f => f.Get<IODataFeature>() ).Returns( odataFeature );
            features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( apiVersioningFeature );
            query.Setup( q => q[It.IsAny<string>()] ).Returns( ( string k ) => queryValues.TryGetValue( k, out var v ) ? v : StringValues.Empty );
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider );
            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );
            httpContext.SetupGet( c => c.Request ).Returns( () => httpRequest.Object );
            httpRequest.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpRequest.SetupProperty( r => r.Method, "GET" );
            httpRequest.SetupProperty( r => r.Scheme, url.Scheme );
            httpRequest.SetupProperty( r => r.Host, new HostString( url.Host ) );
            httpRequest.SetupProperty( r => r.PathBase, new PathString() );
            httpRequest.SetupProperty( r => r.Path, new PathString( '/' + url.GetComponents( Path, Unescaped ) ) );
            httpRequest.SetupProperty( r => r.QueryString, new QueryString( url.Query ) );
            httpRequest.SetupGet( r => r.Query ).Returns( query.Object );

            return httpContext.Object;
        }
    }
}