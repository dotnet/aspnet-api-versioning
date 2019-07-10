namespace Microsoft.AspNetCore.Mvc.Routing
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using Xunit;
    using static System.Threading.Tasks.Task;

    public partial class ApiVersionMatcherPolicyTest
    {
        [Fact]
        public void applies_to_endpoints_should_return_true_for_api_versioned_actions()
        {
            // arrange
            var policy = new ApiVersionMatcherPolicy( NewDefaultOptions(), NewReporter(), NewLoggerFactory() );
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    Properties = { [typeof(ApiVersionModel)] = ApiVersionModel.Default },
                },
            };
            var endpoints = new[]
            {
                new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default ),
            };

            // act
            var result = policy.AppliesToEndpoints( endpoints );

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void applies_to_endpoints_should_return_false_for_normal_actions()
        {
            // arrange
            var policy = new ApiVersionMatcherPolicy( NewDefaultOptions(), NewReporter(), NewLoggerFactory() );
            var items = new object[] { new ActionDescriptor() };
            var endpoints = new[]
            {
                new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default ),
            };

            // act
            var result = policy.AppliesToEndpoints( endpoints );

            // assert
            result.Should().BeFalse();
        }

        static IOptions<ApiVersioningOptions> NewDefaultOptions() => Options.Create( new ApiVersioningOptions() );

        static IReportApiVersions NewReporter() => Mock.Of<IReportApiVersions>();

        static ILoggerFactory NewLoggerFactory()
        {
            var logger = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();

            logger.Setup( l => l.IsEnabled( It.IsAny<LogLevel>() ) ).Returns( false );
            loggerFactory.Setup( lf => lf.CreateLogger( It.IsAny<string>() ) ).Returns( logger.Object );

            return loggerFactory.Object;
        }

        static HttpContext NewHttpContext( Mock<IApiVersioningFeature> apiVersioningFeature )
        {
            var features = new FeatureCollection();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();
            var httpContext = new Mock<HttpContext>();
            var routingFeature = new Mock<IRoutingFeature>();

            routingFeature.SetupProperty( r => r.RouteData, new RouteData() );
            features.Set( apiVersioningFeature.Object );
            features.Set( routingFeature.Object );
            request.SetupProperty( r => r.Scheme, Uri.UriSchemeHttp );
            request.SetupProperty( r => r.Host, new HostString( "tempuri.org" ) );
            request.SetupProperty( r => r.Path, PathString.Empty );
            request.SetupProperty( r => r.PathBase, PathString.Empty );
            request.SetupProperty( r => r.QueryString, QueryString.Empty );
            request.SetupProperty( r => r.Method, "GET" );
            response.SetupGet( r => r.Headers ).Returns( Mock.Of<IHeaderDictionary>() );
            httpContext.SetupGet( hc => hc.Features ).Returns( features );
            httpContext.SetupGet( hc => hc.Request ).Returns( request.Object );
            httpContext.SetupGet( hc => hc.Response ).Returns( response.Object );

            return httpContext.Object;
        }
    }
}