namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Builder.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.Web.OData.Routing;
    using Moq;
    using System;
    using System.Diagnostics;
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
            var model = new EdmModel();
            var httpContext = NewHttpContext( url, model, Default );
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
            var url = new Uri( $"http://localhost/Tests(1)?api-version={apiVersionValue}" );
            var apiVersion = Parse( apiVersionValue );
            var context = NewHttpContext( url, Test.Model, Parse( apiVersionValue ) );
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
            var url = new Uri( requestUri );
            var apiVersion = Parse( apiVersionValue );
            var context = NewHttpContext( url, Test.EmptyModel, apiVersion );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { ["odataPath"] = odataPath };
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, false )]
        public void match_should_return_expected_result_when_controller_is_implicitly_versioned( bool allowImplicitVersioning, bool expected )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );

            void OnConfigure( ApiVersioningOptions options )
            {
                options.DefaultApiVersion = apiVersion;
                options.AssumeDefaultVersionWhenUnspecified = allowImplicitVersioning;
            }

            var url = new Uri( $"http://localhost/Tests(1)" );
            var context = NewHttpContext( url, Test.Model, apiVersion, configure: OnConfigure );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { ["odataPath"] = "Tests(1)" };
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            var result = constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            result.Should().Be( expected );
        }

        [Fact]
        public void match_should_return_400_when_requested_api_version_is_ambiguous()
        {
            // arrange
            var url = new Uri( $"http://localhost/Tests(1)?api-version=1.0&api-version=2.0" );
            var apiVersion = new ApiVersion( 1, 0 );
            var route = Mock.Of<IRouter>();
            var routeKey = (string) null;
            var values = new RouteValueDictionary() { { "odataPath", "Tests(1)" } };
            var context = NewHttpContext( url, Test.Model, apiVersion );
            var constraint = new VersionedODataPathRouteConstraint( "odata", apiVersion );

            // act
            Action match = () => constraint.Match( context, route, routeKey, values, IncomingRequest );

            // assert
            throw new Exception( "Test setup incomplete" );
            //match.ShouldThrow<HttpResponseException>().And.Response.StatusCode.Should().Be( BadRequest );
        }

        static HttpContext NewHttpContext( Uri url, IEdmModel model, ApiVersion apiVersion, string routePrefix = null, Action<ApiVersioningOptions> configure = null )
        {
            var features = new Mock<IFeatureCollection>();
            var odataFeature = Mock.Of<IODataFeature>();
            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var services = new ServiceCollection();

            services.AddLogging();
            services.Add( Singleton<DiagnosticSource>( new DiagnosticListener( "test" ) ) );
            services.Add( Singleton<IOptions<MvcOptions>>( new OptionsWrapper<MvcOptions>( new MvcOptions() ) ) );
            services.AddMvcCore();
            services.AddApiVersioning( configure ?? ( _ => { } ) );
            services.AddOData().EnableApiVersioning();

            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder( serviceProvider );
            var modelBuilder = serviceProvider.GetRequiredService<VersionedODataModelBuilder>();

            app.UseMvc( rb => rb.MapVersionedODataRoute( "odata", routePrefix, model, apiVersion ) );
            features.SetupGet( f => f[typeof( IODataFeature )] ).Returns( odataFeature );
            features.Setup( f => f.Get<IODataFeature>() ).Returns( odataFeature );
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider );
            httpContext.SetupGet( c => c.Request ).Returns( () => httpRequest.Object );
            httpRequest.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpRequest.SetupProperty( r => r.Method, "GET" );
            httpRequest.SetupProperty( r => r.Protocol, url.Scheme );
            httpRequest.SetupProperty( r => r.Host, new HostString( url.Host ) );
            httpRequest.SetupProperty( r => r.Path, new PathString( url.GetComponents( Path, Unescaped ) ) );
            httpRequest.SetupProperty( r => r.QueryString, new QueryString( url.Query ) );

            return httpContext.Object;
        }
    }
}