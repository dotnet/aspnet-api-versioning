namespace Microsoft.AspNetCore.Mvc
{
    using Extensions.Primitives;
    using FluentAssertions;
    using Http;
    using Microsoft.AspNetCore.Http.Features;
    using Moq;
    using System;
    using Versioning;
    using Xunit;

    public class HttpContextExtensionsTest
    {
        [Fact]
        public void http_context_should_return_raw_api_version_from_query_string_without_configuration()
        {
            // arrange
            var featureCollection = new FeatureCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
            featureCollection.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext.Object ) );
            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

            var features = httpContext.Object.Features.Get<IApiVersioningFeature>();

            // act
            var result = features.RawRequestedApiVersion;

            // assert
            result.Should().Be( "42.0" );
        }

        [Fact]
        public void http_context_should_return_raw_api_version_using_configured_reader()
        {
            // arrange
            var featureCollection = new FeatureCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var headers = new HeaderDictionary() { ["api-version"] = "42.0" };
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new HeaderApiVersionReader( "api-version" ) );
            featureCollection.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext.Object ) );
            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new HeaderApiVersionReader( "api-version" ) );
            request.SetupGet( r => r.Headers ).Returns( headers );
            request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

            var features = httpContext.Object.Features.Get<IApiVersioningFeature>();

            // act
            var result = features.RawRequestedApiVersion;

            // assert
            result.Should().Be( "42.0" );
        }

        [Fact]
        public void http_context_should_return_requested_api_version()
        {
            // arrange
            var featureCollection = new FeatureCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var version = new ApiVersion( 42, 0 );
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
            featureCollection.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext.Object ) );
            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

            // act
            var result = httpContext.Object.GetRequestedApiVersion();

            // assert
            result.Should().Be( version );
        }

        [Fact]
        public void http_context_should_return_null_api_version_when_the_value_is_invalid()
        {
            // arrange
            var featureCollection = new FeatureCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            featureCollection.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext.Object ) );
            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "abc" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            request.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

            // act
            var result = httpContext.Object.GetRequestedApiVersion();

            // assert
            result.Should().BeNull();
        }
    }
}