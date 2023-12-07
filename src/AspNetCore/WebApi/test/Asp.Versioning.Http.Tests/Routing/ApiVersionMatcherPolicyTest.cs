// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

public class ApiVersionMatcherPolicyTest
{
    [Fact]
    public void applies_to_endpoints_should_return_true_for_api_versioned_endpoints()
    {
        // arrange
        var policy = NewApiVersionMatcherPolicy();
        var items = new object[]
        {
            new ApiVersionMetadata( ApiVersionModel.Default, ApiVersionModel.Default ),
        };
        var endpoints = new Endpoint[] { new( Limbo, new( items ), default ) };

        // act
        var result = policy.AppliesToEndpoints( endpoints );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void applies_to_endpoints_should_return_false_for_normal_endpoints()
    {
        // arrange
        var policy = NewApiVersionMatcherPolicy();
        var endpoints = new Endpoint[] { new( Limbo, new(), default ) };

        // act
        var result = policy.AppliesToEndpoints( endpoints );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void apply_should_use_400_endpoint_for_ambiguous_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersions, new[] { "1.0", "2.0" } );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( new[] { "1.0", "2.0" } ) } );
        var model = new ApiVersionModel(
            declaredVersions: new ApiVersion[] { new( 1, 0 ), new( 2, 0 ) },
            supportedVersions: new ApiVersion[] { new( 1, 0 ), new( 2, 0 ) },
            deprecatedVersions: Enumerable.Empty<ApiVersion>(),
            advertisedVersions: Enumerable.Empty<ApiVersion>(),
            deprecatedAdvertisedVersions: Enumerable.Empty<ApiVersion>() );
        var routePattern = RoutePatternFactory.Parse( "api/values" );
        var builder = new RouteEndpointBuilder( Limbo, routePattern, 0 )
        {
            Metadata = { new ApiVersionMetadata( model, model ) },
        };
        var endpoints = new[] { builder.Build() };
        var edges = policy.GetEdges( endpoints );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );

        // act
        var endpoint = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];

        // assert
        endpoint.DisplayName.Should().Be( "400 Ambiguous API Version" );
    }

    [Fact]
    public async Task apply_should_have_candidate_for_matched_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), default );
        var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, [0] );
        var policy = NewApiVersionMatcherPolicy();

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

        feature.SetupProperty( f => f.RawRequestedApiVersion, "2.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, new[] { "2.0" } );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 2, 0 ) );

        var policy = NewApiVersionMatcherPolicy();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), default );
        var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, [0] );
        var httpContext = NewHttpContext( feature );

        // act
        await policy.ApplyAsync( httpContext, candidates );

        // assert
        httpContext.GetEndpoint().DisplayName.Should().Be( "400 Unsupported API Version" );
    }

    [Fact]
    public void apply_should_use_400_endpoint_for_invalid_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersions, new[] { "blah" } );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( new[] { "blah" } ) } );
        var model = new ApiVersionModel(
            declaredVersions: new ApiVersion[] { new( 1, 0 ) },
            supportedVersions: new ApiVersion[] { new( 1, 0 ) },
            deprecatedVersions: Enumerable.Empty<ApiVersion>(),
            advertisedVersions: Enumerable.Empty<ApiVersion>(),
            deprecatedAdvertisedVersions: Enumerable.Empty<ApiVersion>() );
        var routePattern = RoutePatternFactory.Parse( "api/values" );
        var builder = new RouteEndpointBuilder( Limbo, routePattern, 0 )
        {
            Metadata = { new ApiVersionMetadata( model, model ) },
        };
        var endpoints = new[] { builder.Build() };
        var edges = policy.GetEdges( endpoints );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );

        // act
        var endpoint = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];

        // assert
        endpoint.DisplayName.Should().Be( "400 Invalid API Version" );
    }

    [Fact]
    public async Task apply_should_use_400_endpoint_for_unspecified_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, default );
        feature.SetupProperty( f => f.RawRequestedApiVersions, Array.Empty<string>() );
        feature.SetupProperty( f => f.RequestedApiVersion, default );

        var policy = NewApiVersionMatcherPolicy();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), "Test" );
        var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, [0] );
        var httpContext = NewHttpContext( feature );

        // act
        await policy.ApplyAsync( httpContext, candidates );

        // assert
        httpContext.GetEndpoint().DisplayName.Should().Be( "400 Unspecified API Version" );
    }

    [Fact]
    public async Task apply_should_have_candidate_for_unspecified_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), default );
        var candidates = new CandidateSet( new[] { endpoint }, new[] { new RouteValueDictionary() }, [0] );
        var options = new ApiVersioningOptions() { AssumeDefaultVersionWhenUnspecified = true };
        var policy = NewApiVersionMatcherPolicy( options );

        feature.SetupProperty( f => f.RawRequestedApiVersion, default );
        feature.SetupProperty( f => f.RequestedApiVersion, default );

        var httpContext = NewHttpContext( feature );

        // act
        await policy.ApplyAsync( httpContext, candidates );

        // assert
        candidates.IsValidCandidate( 0 ).Should().BeTrue();
        feature.Object.RequestedApiVersion.Should().Be( new ApiVersion( 1, 0 ) );
    }

    private static Task Limbo( HttpContext context ) => Task.CompletedTask;

    private static ApiVersionMatcherPolicy NewApiVersionMatcherPolicy( ApiVersioningOptions options = default ) =>
        new(
            ApiVersionParser.Default,
            Enumerable.Empty<IApiVersionMetadataCollationProvider>(),
            Options.Create( options ?? new() ),
            Mock.Of<ILogger<ApiVersionMatcherPolicy>>() );

    private static HttpContext NewHttpContext(
        Mock<IApiVersioningFeature> apiVersioningFeature,
        IServiceProvider services = default,
        Dictionary<string, StringValues> queryParameters = default )
    {
        var features = new FeatureCollection();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var httpContext = new Mock<HttpContext>();
        var routingFeature = new Mock<IRoutingFeature>();
        QueryString queryString;
        QueryCollection query;

        if ( queryParameters is null || queryParameters.Count == 0 )
        {
            queryString = QueryString.Empty;
            query = new QueryCollection();
        }
        else
        {
            queryString = QueryString.Create( queryParameters );
            query = new QueryCollection( queryParameters );
        }

        routingFeature.SetupProperty( r => r.RouteData, new RouteData() );
        features.Set( apiVersioningFeature.Object );
        features.Set( routingFeature.Object );
        request.SetupProperty( r => r.Scheme, Uri.UriSchemeHttp );
        request.SetupProperty( r => r.Host, new HostString( "tempuri.org" ) );
        request.SetupProperty( r => r.Path, PathString.Empty );
        request.SetupProperty( r => r.PathBase, PathString.Empty );
        request.SetupProperty( r => r.Query, query );
        request.SetupProperty( r => r.QueryString, queryString );
        request.SetupProperty( r => r.Method, "GET" );
        response.SetupGet( r => r.Headers ).Returns( Mock.Of<IHeaderDictionary>() );
        httpContext.SetupGet( hc => hc.Features ).Returns( features );
        httpContext.SetupGet( hc => hc.Request ).Returns( request.Object );
        httpContext.SetupGet( hc => hc.Response ).Returns( response.Object );

        if ( services is not null )
        {
            httpContext.SetupProperty( hc => hc.RequestServices, services );
        }

        return httpContext.Object;
    }
}