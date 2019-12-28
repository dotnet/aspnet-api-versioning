namespace Microsoft.AspNetCore.Mvc.Routing
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Matching;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static Moq.Times;
    using static System.Array;
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

        [Fact]
        public async Task apply_should_use_400_endpoint_for_ambiguous_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var errorResponses = new Mock<IErrorResponseProvider>();
            var result = new Mock<IActionResult>();

            feature.SetupGet( f => f.RequestedApiVersion ).Throws( new AmbiguousApiVersionException( "Test", new[] { "1.0", "2.0" } ) );
            result.Setup( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ) ).Returns( CompletedTask );
            errorResponses.Setup( er => er.CreateResponse( It.IsAny<ErrorResponseContext>() ) ).Returns( result.Object );

            var options = Options.Create( new ApiVersioningOptions() { ErrorResponses = errorResponses.Object } );
            var policy = new ApiVersionMatcherPolicy( options, NewReporter(), NewLoggerFactory() );
            var httpContext = NewHttpContext( feature );
            var candidates = new CandidateSet( Empty<Endpoint>(), Empty<RouteValueDictionary>(), Empty<int>() );

            // act
            await policy.ApplyAsync( httpContext, candidates );
            await httpContext.GetEndpoint().RequestDelegate( httpContext );

            // assert
            result.Verify( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ), Once() );
            errorResponses.Verify( er => er.CreateResponse( It.Is<ErrorResponseContext>( c => c.StatusCode == 400 && c.ErrorCode == "AmbiguousApiVersion" ) ), Once() );
        }

        [Fact]
        public async Task apply_should_have_candidate_for_matched_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    Properties = { [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ) },
                },
            };
            var endpoint = new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default );
            var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, new[] { 0 } );
            var policy = new ApiVersionMatcherPolicy( NewDefaultOptions(), NewReporter(), NewLoggerFactory() );

            feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

            var httpContext = NewHttpContext( feature );

            // act
            await policy.ApplyAsync( httpContext, candidates );

            // assert
            candidates.IsValidCandidate( 0 ).Should().BeTrue();
        }

        [Fact]
        public async Task apply_should_use_400_endpoint_for_unmatched_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var errorResponses = new Mock<IErrorResponseProvider>();
            var result = new Mock<IActionResult>();

            feature.SetupProperty( f => f.RawRequestedApiVersion, "2.0" );
            feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 2, 0 ) );
            result.Setup( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ) ).Returns( CompletedTask );
            errorResponses.Setup( er => er.CreateResponse( It.IsAny<ErrorResponseContext>() ) ).Returns( result.Object );

            var options = Options.Create( new ApiVersioningOptions() { ErrorResponses = errorResponses.Object } );
            var policy = new ApiVersionMatcherPolicy( options, NewReporter(), NewLoggerFactory() );
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    DisplayName = "Test",
                    ActionConstraints = new IActionConstraintMetadata[]{ new HttpMethodActionConstraint(new[] { "GET" }) },
                    Properties = { [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ) },
                },
            };
            var endpoint = new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default );
            var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, new[] { 0 } );
            var httpContext = NewHttpContext( feature );

            // act
            await policy.ApplyAsync( httpContext, candidates );
            await httpContext.GetEndpoint().RequestDelegate( httpContext );

            // assert
            result.Verify( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ), Once() );
            errorResponses.Verify( er => er.CreateResponse( It.Is<ErrorResponseContext>( c => c.StatusCode == 400 && c.ErrorCode == "UnsupportedApiVersion" ) ), Once() );
        }

        [Fact]
        public async Task apply_should_use_400_endpoint_for_invalid_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var errorResponses = new Mock<IErrorResponseProvider>();
            var result = new Mock<IActionResult>();

            feature.SetupProperty( f => f.RawRequestedApiVersion, "blah" );
            feature.SetupProperty( f => f.RequestedApiVersion, default );
            result.Setup( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ) ).Returns( CompletedTask );
            errorResponses.Setup( er => er.CreateResponse( It.IsAny<ErrorResponseContext>() ) ).Returns( result.Object );

            var options = Options.Create( new ApiVersioningOptions() { ErrorResponses = errorResponses.Object } );
            var policy = new ApiVersionMatcherPolicy( options, NewReporter(), NewLoggerFactory() );
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    DisplayName = "Test",
                    ActionConstraints = new IActionConstraintMetadata[]{ new HttpMethodActionConstraint(new[] { "GET" }) },
                    Properties = { [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ) },
                },
            };
            var endpoint = new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default );
            var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, new[] { 0 } );
            var httpContext = NewHttpContext( feature );

            // act
            await policy.ApplyAsync( httpContext, candidates );
            await httpContext.GetEndpoint().RequestDelegate( httpContext );

            // assert
            result.Verify( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ), Once() );
            errorResponses.Verify( er => er.CreateResponse( It.Is<ErrorResponseContext>( c => c.StatusCode == 400 && c.ErrorCode == "InvalidApiVersion" ) ), Once() );
        }

        [Fact]
        public async Task apply_should_use_400_endpoint_for_unspecified_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var errorResponses = new Mock<IErrorResponseProvider>();
            var result = new Mock<IActionResult>();

            feature.SetupProperty( f => f.RawRequestedApiVersion, default );
            feature.SetupProperty( f => f.RequestedApiVersion, default );
            result.Setup( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ) ).Returns( CompletedTask );
            errorResponses.Setup( er => er.CreateResponse( It.IsAny<ErrorResponseContext>() ) ).Returns( result.Object );

            var options = Options.Create( new ApiVersioningOptions() { ErrorResponses = errorResponses.Object } );
            var policy = new ApiVersionMatcherPolicy( options, NewReporter(), NewLoggerFactory() );
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    DisplayName = "Test",
                    ActionConstraints = new IActionConstraintMetadata[]{ new HttpMethodActionConstraint(new[] { "GET" }) },
                    Properties = { [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ) },
                },
            };
            var endpoint = new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default );
            var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, new[] { 0 } );
            var httpContext = NewHttpContext( feature );

            // act
            await policy.ApplyAsync( httpContext, candidates );
            await httpContext.GetEndpoint().RequestDelegate( httpContext );

            // assert
            result.Verify( r => r.ExecuteResultAsync( It.IsAny<ActionContext>() ), Once() );
            errorResponses.Verify( er => er.CreateResponse( It.Is<ErrorResponseContext>( c => c.StatusCode == 400 && c.ErrorCode == "ApiVersionUnspecified" ) ), Once() );
        }

        [Fact]
        public async Task apply_should_have_candidate_for_unspecified_api_version()
        {
            // arrange
            var feature = new Mock<IApiVersioningFeature>();
            var items = new object[]
            {
                new ActionDescriptor()
                {
                    Properties = { [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ) },
                },
            };
            var endpoint = new Endpoint( c => CompletedTask, new EndpointMetadataCollection( items ), default );
            var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, new[] { 0 } );
            var options = Options.Create( new ApiVersioningOptions() { AssumeDefaultVersionWhenUnspecified = true } );
            var policy = new ApiVersionMatcherPolicy( options, NewReporter(), NewLoggerFactory() );

            feature.SetupProperty( f => f.RawRequestedApiVersion, default );
            feature.SetupProperty( f => f.RequestedApiVersion, default );

            var httpContext = NewHttpContext( feature );

            // act
            await policy.ApplyAsync( httpContext, candidates );

            // assert
            candidates.IsValidCandidate( 0 ).Should().BeTrue();
            feature.Object.RequestedApiVersion.Should().Be( new ApiVersion( 1, 0 ) );
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