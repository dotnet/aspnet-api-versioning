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

        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0", "2.0"] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( ["1.0", "2.0"] ) } );
        var model = new ApiVersionModel(
            declaredVersions: [new( 1, 0 ), new( 2, 0 )],
            supportedVersions: [new( 1, 0 ), new( 2, 0 )],
            deprecatedVersions: [],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
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
    public async Task jump_table_should_use_introduced_endpoint_for_controller_declared_version_before_action()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( "1.0" ) } );
        var v1 = new ApiVersion( 1, 0 );
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );
        var apiModel = new ApiVersionModel(
            declaredVersions: [v1, v2, v3],
            supportedVersions: [v1, v2, v3],
            deprecatedVersions: [],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
        var endpointModel = new ApiVersionModel(
            declaredVersions: [v2, v3],
            supportedVersions: [v2, v3],
            deprecatedVersions: [],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
        var routePattern = RoutePatternFactory.Parse( "api/values" );
        var builder = new RouteEndpointBuilder( Limbo, routePattern, 0 )
        {
            Metadata =
            {
                new ApiVersionMetadata( apiModel, endpointModel, introducedInApiVersions: [new( v2, 404 )] ),
            },
        };
        var endpoints = new[] { builder.Build() };
        var edges = policy.GetEdges( endpoints );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var endpoint = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];
        var responseContext = new DefaultHttpContext();

        // act
        await endpoint.RequestDelegate!( responseContext );

        // assert
        responseContext.Response.StatusCode.Should().Be( 404 );
    }

    [Fact]
    public async Task jump_table_should_use_configured_status_code_for_introduced_status_code_zero()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            UnsupportedApiVersionStatusCode = 410,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( "1.0" ) } );
        var endpoint = NewIntroducedEndpoint( IntroducedInApiVersionAttribute.UseConfiguredStatusCode );
        var edges = policy.GetEdges( [endpoint] );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];
        var responseContext = new DefaultHttpContext();

        // act
        await selected.RequestDelegate!( responseContext );

        // assert
        selected.DisplayName.Should().Be( "410 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 410 );
    }

    [Fact]
    public async Task apply_should_use_configured_status_code_for_introduced_status_code_zero()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var options = new ApiVersioningOptions()
        {
            UnsupportedApiVersionStatusCode = 410,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var endpoint = NewIntroducedEndpoint( IntroducedInApiVersionAttribute.UseConfiguredStatusCode );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
        var httpContext = NewHttpContext( feature );
        var responseContext = new DefaultHttpContext();

        // act
        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( responseContext );

        // assert
        httpContext.GetEndpoint().DisplayName.Should().Be( "410 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 410 );
    }

    [Fact]
    public async Task apply_should_use_introduced_endpoint_for_controller_declared_version_before_action()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var policy = NewApiVersionMatcherPolicy();
        var endpoint = NewIntroducedEndpoint( 404 );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
        var httpContext = NewHttpContext( feature );
        var responseContext = new DefaultHttpContext();

        // act
        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( responseContext );

        // assert
        httpContext.GetEndpoint().DisplayName.Should().Be( "404 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 404 );
    }

    [Theory]
    [InlineData( "1.0" )]
    [InlineData( "2.0" )]
    public async Task jump_table_should_use_latest_matching_introduced_version( string requestedVersion )
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, requestedVersion );
        feature.SetupProperty( f => f.RawRequestedApiVersions, [requestedVersion] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( requestedVersion ) } );
        var endpoint = NewIntroducedEndpoint( [new( new ApiVersion( 2, 0 ), 409 ), new( new ApiVersion( 3, 0 ), 410 )], implementedVersion: new( 3, 0 ) );
        var edges = policy.GetEdges( [endpoint] );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];
        var responseContext = new DefaultHttpContext();

        // act
        await selected.RequestDelegate!( responseContext );

        // assert
        selected.DisplayName.Should().Be( "410 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 410 );
    }

    [Fact]
    public async Task jump_table_should_use_smallest_status_code_for_same_introduced_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( "1.0" ) } );
        var v2 = new ApiVersion( 2, 0 );
        var first = NewIntroducedEndpoint( [new( v2, 410 )], implementedVersion: v2 );
        var second = NewIntroducedEndpoint( [new( v2, 404 )], implementedVersion: v2 );
        var edges = policy.GetEdges( [first, second] );
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];
        var responseContext = new DefaultHttpContext();

        // act
        await selected.RequestDelegate!( responseContext );

        // assert
        selected.DisplayName.Should().Be( "404 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 404 );
    }

    [Fact]
    public void edge_key_equals_should_compare_introduced_later_status_code()
    {
        // arrange
        var keyType = typeof( ApiVersionMatcherPolicy ).Assembly.GetType( "Asp.Versioning.Routing.EdgeKey" );
        var ctor = keyType!.GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            [typeof( ApiVersion ), typeof( int ), typeof( ApiVersionMetadata ), typeof( HashSet<RoutePattern> )],
            modifiers: null );
        var apiVersion = new ApiVersion( 2.0 );
        var metadata = ApiVersionMetadata.Empty;
        var routePatterns = new HashSet<RoutePattern>();
        var left = ctor!.Invoke( [apiVersion, 404, metadata, routePatterns] );
        var same = ctor.Invoke( [apiVersion, 404, metadata, routePatterns] );
        var differentStatusCode = ctor.Invoke( [apiVersion, 410, metadata, routePatterns] );

        // act
        var sameResult = left.Equals( same );
        var differentResult = left.Equals( differentStatusCode );

        // assert
        sameResult.Should().BeTrue();
        differentResult.Should().BeFalse();
    }

    [Fact]
    public async Task apply_should_have_candidate_for_matched_api_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), default );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
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
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["2.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 2, 0 ) );

        var policy = NewApiVersionMatcherPolicy();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), default );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
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

        feature.SetupProperty( f => f.RawRequestedApiVersions, ["blah"] );

        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( feature, queryParameters: new() { ["api-version"] = new( ["blah"] ) } );
        var model = new ApiVersionModel(
            declaredVersions: [new( 1, 0 )],
            supportedVersions: [new( 1, 0 )],
            deprecatedVersions: [],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
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
        feature.SetupProperty( f => f.RawRequestedApiVersions, [] );
        feature.SetupProperty( f => f.RequestedApiVersion, default );

        var policy = NewApiVersionMatcherPolicy();
        var model = new ApiVersionModel( new ApiVersion( 1, 0 ) );
        var items = new object[] { new ApiVersionMetadata( model, model ) };
        var endpoint = new Endpoint( Limbo, new( items ), "Test" );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
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
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
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

    private static RouteEndpoint NewIntroducedEndpoint( int statusCode )
    {
        var v1 = new ApiVersion( 1, 0 );
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );
        var apiModel = new ApiVersionModel( [v1, v2, v3], [v1, v2, v3], [], [], [] );
        var endpointModel = new ApiVersionModel( [v2, v3], [v2, v3], [], [], [] );
        var routePattern = RoutePatternFactory.Parse( "api/values" );
        var builder = new RouteEndpointBuilder( Limbo, routePattern, 0 )
        {
            Metadata =
            {
                new ApiVersionMetadata( apiModel, endpointModel, introducedInApiVersions: [new( v2, statusCode )] ),
            },
        };

        return (RouteEndpoint) builder.Build();
    }

    private static RouteEndpoint NewIntroducedEndpoint( IntroducedInApiVersionMetadata[] introduced, ApiVersion implementedVersion )
    {
        var v1 = new ApiVersion( 1, 0 );
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );
        var apiModel = new ApiVersionModel( [v1, v2, v3], [v1, v2, v3], [], [], [] );
        var endpointModel = new ApiVersionModel( [implementedVersion], [implementedVersion], [], [], [] );
        var routePattern = RoutePatternFactory.Parse( "api/values" );
        var builder = new RouteEndpointBuilder( Limbo, routePattern, 0 )
        {
            Metadata =
            {
                new ApiVersionMetadata( apiModel, endpointModel, introducedInApiVersions: introduced ),
            },
        };

        return (RouteEndpoint) builder.Build();
    }

    private static ApiVersionMatcherPolicy NewApiVersionMatcherPolicy( ApiVersioningOptions options = default ) =>
        new(
            ApiVersionParser.Default,
            [],
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