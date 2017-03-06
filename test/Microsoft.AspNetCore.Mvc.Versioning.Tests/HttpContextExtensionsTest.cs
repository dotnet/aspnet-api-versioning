namespace Microsoft.AspNetCore.Mvc
{
    using Extensions.Primitives;
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Versioning;
    using Xunit;

    public class HttpContextExtensionsTest
    {
        [Fact]
        public void http_context_should_return_raw_api_version_from_query_string_without_configuration()
        {
            // arrange
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var items = new Dictionary<object, object>();

            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, Mock.Of<IServiceProvider>() );
            httpContext.SetupProperty( c => c.Items, items );
            items["MS_ApiVersionRequestProperties"] = new ApiVersionRequestProperties( httpContext.Object );

            // act
            var result = httpContext.Object.ApiVersionProperties().RawApiVersion;

            // assert
            result.Should().Be( "42.0" );
        }

        [Fact]
        public void http_context_should_return_raw_api_version_using_configured_reader()
        {
            // arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var headers = new HeaderDictionary() { ["api-version"] = "42.0" };
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var items = new Dictionary<object, object>();

            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new HeaderApiVersionReader( "api-version" ) );
            request.SetupGet( r => r.Headers ).Returns( headers );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );
            httpContext.SetupProperty( c => c.Items, items );
            items["MS_ApiVersionRequestProperties"] = new ApiVersionRequestProperties( httpContext.Object );

            // act
            var result = httpContext.Object.ApiVersionProperties().RawApiVersion;

            // assert
            result.Should().Be( "42.0" );
        }

        [Fact]
        public void http_context_should_return_requested_api_version()
        {
            // arrange
            var version = new ApiVersion( 42, 0 );
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );
            httpContext.SetupProperty( c => c.RequestServices, Mock.Of<IServiceProvider>() );

            // act
            var result = httpContext.Object.GetRequestedApiVersion();

            // assert
            result.Should().Be( version );
        }

        [Fact]
        public void http_context_should_return_null_api_version_when_the_value_is_invalid()
        {
            // arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "abc" ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );
            httpContext.SetupGet( c => c.Request ).Returns( request.Object );
            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

            // act
            var result = httpContext.Object.GetRequestedApiVersion();

            // assert
            result.Should().BeNull();
        }
    }
}
