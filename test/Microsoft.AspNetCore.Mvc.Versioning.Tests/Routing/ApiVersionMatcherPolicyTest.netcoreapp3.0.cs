#if NETCOREAPP
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
    }
}
#endif