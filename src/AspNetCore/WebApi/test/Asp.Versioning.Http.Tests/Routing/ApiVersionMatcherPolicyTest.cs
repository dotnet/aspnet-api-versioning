// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

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
    public async Task jump_table_should_write_problem_details_for_introduced_endpoint()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = true,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( "1.0", options );
        var endpoint = NewIntroducedEndpoint( 404 );
        var edges = policy.GetEdges( [endpoint] );
        var tableEdges = NewJumpTableEdges( edges );
        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];

        // act
        await selected.RequestDelegate!( httpContext );
        await httpContext.Response.CompleteAsync();

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 404 );
        httpContext.Response.Headers["api-supported-versions"].Should().Equal( "2.0, 3.0" );
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

    [Fact]
    public async Task apply_should_write_problem_details_for_introduced_endpoint()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var options = new ApiVersioningOptions()
        {
            ReportApiVersions = true,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var endpoint = NewIntroducedEndpoint( 404 );
        var candidates = new CandidateSet( [endpoint], [[]], [0] );
        var httpContext = NewHttpContext( "1.0", options );

        httpContext.Features.Set( feature.Object );

        // act
        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( httpContext );

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 404 );
        httpContext.Response.Headers["api-supported-versions"].Should().Equal( "2.0, 3.0" );
    }

    [Fact]
    public async Task introduced_endpoint_should_write_same_problem_details_from_jump_table_and_apply()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = true,
        };

        // act
        var fast = await InvokeJumpTableIntroducedEndpoint( options );
        var slow = await InvokeApplyIntroducedEndpoint( options );

        // assert
        ( await ReadResponseBody( fast ) ).Should().Be( await ReadResponseBody( slow ) );
        fast.Response.Headers["api-supported-versions"].Should().Equal( slow.Response.Headers["api-supported-versions"] );
    }

    [Fact]
    public async Task jump_table_should_not_report_api_versions_for_introduced_endpoint_when_disabled()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = false,
        };

        // act
        var httpContext = await InvokeJumpTableIntroducedEndpoint( options );

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 404 );
        httpContext.Response.Headers.ContainsKey( "api-supported-versions" ).Should().BeFalse();
        httpContext.Response.Headers.ContainsKey( "api-deprecated-versions" ).Should().BeFalse();
    }

    [Fact]
    public async Task apply_should_not_report_api_versions_for_introduced_endpoint_when_disabled()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ReportApiVersions = false,
        };

        // act
        var httpContext = await InvokeApplyIntroducedEndpoint( options );

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 404 );
        httpContext.Response.Headers.ContainsKey( "api-supported-versions" ).Should().BeFalse();
        httpContext.Response.Headers.ContainsKey( "api-deprecated-versions" ).Should().BeFalse();
    }

    [Fact]
    public async Task apply_should_use_smallest_status_code_for_same_introduced_version()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var policy = NewApiVersionMatcherPolicy();
        var v2 = new ApiVersion( 2, 0 );
        var first = NewIntroducedEndpoint( [new( v2, 410 )], implementedVersion: v2 );
        var second = NewIntroducedEndpoint( [new( v2, 404 )], implementedVersion: v2 );
        var candidates = new CandidateSet( [first, second], [[], []], [0, 0] );
        var httpContext = NewHttpContext( feature );
        var responseContext = new DefaultHttpContext();

        // act
        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( responseContext );

        // assert
        httpContext.GetEndpoint().DisplayName.Should().Be( "404 Introduced API Version" );
        responseContext.Response.StatusCode.Should().Be( 404 );
    }

    [Fact]
    public async Task apply_should_use_latest_introduced_version_across_candidates()
    {
        // arrange
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var options = new ApiVersioningOptions()
        {
            ReportApiVersions = true,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );
        var first = NewIntroducedEndpoint( [new( v2, 404 )], implementedVersion: v2 );
        var second = NewIntroducedEndpoint( [new( v3, 410 )], implementedVersion: v3 );
        var candidates = new CandidateSet( [first, second], [[], []], [0, 0] );
        var httpContext = NewHttpContext( "1.0", options );

        httpContext.Features.Set( feature.Object );

        // act
        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( httpContext );
        await httpContext.Response.CompleteAsync();

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 410, "3.0" );
    }

    [Fact]
    public async Task jump_table_should_use_latest_introduced_version_across_candidates()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = true,
        };
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( "1.0", options );
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );
        var first = NewIntroducedEndpoint( [new( v2, 404 )], implementedVersion: v2 );
        var second = NewIntroducedEndpoint( [new( v3, 410 )], implementedVersion: v3 );
        var edges = policy.GetEdges( [first, second] );
        var jumpTable = policy.BuildJumpTable( 42, NewJumpTableEdges( edges ) );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];

        // act
        await selected.RequestDelegate!( httpContext );
        await httpContext.Response.CompleteAsync();

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( httpContext, 410, "3.0" );
    }

    [Fact]
    public async Task introduced_endpoint_should_use_same_latest_introduced_version_from_jump_table_and_apply()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = true,
        };
        var endpoints = NewIntroducedEndpointCandidates( latestStatusCode: 410, earlierStatusCode: 404 );

        // act
        var fast = await InvokeJumpTableIntroducedEndpoint( options, endpoints );
        var slow = await InvokeApplyIntroducedEndpoint( options, endpoints );

        // assert
        ( await ReadResponseBody( fast ) ).Should().Be( await ReadResponseBody( slow ) );
        fast.Response.StatusCode.Should().Be( slow.Response.StatusCode );
        fast.Response.Headers["api-supported-versions"].Should().Equal( slow.Response.Headers["api-supported-versions"] );
    }

    [Fact]
    public async Task introduced_endpoint_should_tie_break_latest_introduced_version_by_smallest_status_code()
    {
        // arrange
        var options = new ApiVersioningOptions()
        {
            ApiVersionReader = new QueryStringApiVersionReader(),
            ReportApiVersions = true,
        };
        var endpoints = NewIntroducedEndpointCandidatesWithLatestTie();

        // act
        var fast = await InvokeJumpTableIntroducedEndpoint( options, endpoints );
        var slow = await InvokeApplyIntroducedEndpoint( options, endpoints );

        // assert
        await ResponseShouldHaveIntroducedProblemDetails( fast, 404, "3.0" );
        await ResponseShouldHaveIntroducedProblemDetails( slow, 404, "3.0" );
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
            [typeof( ApiVersion ), typeof( int ), typeof( ApiVersion ), typeof( ApiVersionMetadata ), typeof( HashSet<RoutePattern> )],
            modifiers: null );
        var apiVersion = new ApiVersion( 2.0 );
        var introducedIn = new ApiVersion( 3.0 );
        var metadata = ApiVersionMetadata.Empty;
        var routePatterns = new HashSet<RoutePattern>();
        var left = ctor!.Invoke( [apiVersion, 404, introducedIn, metadata, routePatterns] );
        var same = ctor.Invoke( [apiVersion, 404, introducedIn, metadata, routePatterns] );
        var differentStatusCode = ctor.Invoke( [apiVersion, 410, introducedIn, metadata, routePatterns] );
        var differentIntroducedIn = ctor.Invoke( [apiVersion, 404, new ApiVersion( 4.0 ), metadata, routePatterns] );

        // act
        var sameResult = left.Equals( same );
        var differentResult = left.Equals( differentStatusCode );
        var differentIntroducedInResult = left.Equals( differentIntroducedIn );

        // assert
        sameResult.Should().BeTrue();
        differentResult.Should().BeFalse();
        differentIntroducedInResult.Should().BeFalse();
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

    private static RouteEndpoint[] NewIntroducedEndpointCandidates( int latestStatusCode, int earlierStatusCode )
    {
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );

        return
        [
            NewIntroducedEndpoint( [new( v2, earlierStatusCode )], implementedVersion: v2 ),
            NewIntroducedEndpoint( [new( v3, latestStatusCode )], implementedVersion: v3 ),
        ];
    }

    private static RouteEndpoint[] NewIntroducedEndpointCandidatesWithLatestTie()
    {
        var v2 = new ApiVersion( 2, 0 );
        var v3 = new ApiVersion( 3, 0 );

        return
        [
            NewIntroducedEndpoint( [new( v2, 400 )], implementedVersion: v2 ),
            NewIntroducedEndpoint( [new( v3, 410 )], implementedVersion: v3 ),
            NewIntroducedEndpoint( [new( v3, 404 )], implementedVersion: v3 ),
        ];
    }

    private static ApiVersionMatcherPolicy NewApiVersionMatcherPolicy( ApiVersioningOptions options = default ) =>
        new(
            ApiVersionParser.Default,
            [],
            Options.Create( options ?? new() ),
            Mock.Of<ILogger<ApiVersionMatcherPolicy>>() );

    private static List<PolicyJumpTableEdge> NewJumpTableEdges( IReadOnlyList<PolicyNodeEdge> edges )
    {
        var tableEdges = new List<PolicyJumpTableEdge>();

        for ( var i = 0; i < edges.Count; i++ )
        {
            tableEdges.Add( new( edges[i].State, i ) );
        }

        return tableEdges;
    }

    private static DefaultHttpContext NewHttpContext( string apiVersion, ApiVersioningOptions options )
    {
        var services = new ServiceCollection();

        services.AddSingleton<IProblemDetailsService>( new TestProblemDetailsService() );
        services.AddSingleton<IReportApiVersions>( new TestApiVersionReporter() );
        services.AddSingleton<IApiVersionReader>( options.ApiVersionReader );
        services.AddSingleton<IApiVersionParser>( ApiVersionParser.Default );

        var context = new DefaultHttpContext()
        {
            RequestServices = services.BuildServiceProvider(),
        };

        context.Request.Scheme = Uri.UriSchemeHttp;
        context.Request.Host = new( "tempuri.org" );
        context.Request.Path = "/api/values";
        context.Response.Body = new MemoryStream();
        context.ApiVersioningFeature.RawRequestedApiVersion = apiVersion;

        return context;
    }

    private static async Task<DefaultHttpContext> InvokeJumpTableIntroducedEndpoint( ApiVersioningOptions options )
    {
        return await InvokeJumpTableIntroducedEndpoint( options, [NewIntroducedEndpoint( 404 )] );
    }

    private static async Task<DefaultHttpContext> InvokeJumpTableIntroducedEndpoint(
        ApiVersioningOptions options,
        RouteEndpoint[] endpoints )
    {
        var policy = NewApiVersionMatcherPolicy( options );
        var httpContext = NewHttpContext( "1.0", options );
        var edges = policy.GetEdges( endpoints );
        var tableEdges = NewJumpTableEdges( edges );
        var jumpTable = policy.BuildJumpTable( 42, tableEdges );
        var selected = edges[jumpTable.GetDestination( httpContext )].Endpoints[0];

        await selected.RequestDelegate!( httpContext );
        await httpContext.Response.CompleteAsync();

        return httpContext;
    }

    private static async Task<DefaultHttpContext> InvokeApplyIntroducedEndpoint( ApiVersioningOptions options )
    {
        return await InvokeApplyIntroducedEndpoint( options, [NewIntroducedEndpoint( 404 )] );
    }

    private static async Task<DefaultHttpContext> InvokeApplyIntroducedEndpoint(
        ApiVersioningOptions options,
        RouteEndpoint[] endpoints )
    {
        var feature = new Mock<IApiVersioningFeature>();

        feature.SetupProperty( f => f.RawRequestedApiVersion, "1.0" );
        feature.SetupProperty( f => f.RawRequestedApiVersions, ["1.0"] );
        feature.SetupProperty( f => f.RequestedApiVersion, new ApiVersion( 1, 0 ) );

        var policy = NewApiVersionMatcherPolicy( options );
        var candidates = new CandidateSet(
            endpoints,
            endpoints.Select( _ => new RouteValueDictionary() ).ToArray(),
            endpoints.Select( _ => 0 ).ToArray() );
        var httpContext = NewHttpContext( "1.0", options );

        httpContext.Features.Set( feature.Object );

        await policy.ApplyAsync( httpContext, candidates );
        await httpContext.GetEndpoint().RequestDelegate!( httpContext );
        await httpContext.Response.CompleteAsync();

        return httpContext;
    }

    private static async Task<string> ReadResponseBody( DefaultHttpContext context )
    {
        context.Response.Body.Position = 0;

        using var reader = new StreamReader( context.Response.Body, leaveOpen: true );

        return await reader.ReadToEndAsync();
    }

    private static async Task ResponseShouldHaveIntroducedProblemDetails( DefaultHttpContext context, int statusCode ) =>
        await ResponseShouldHaveIntroducedProblemDetails( context, statusCode, "2.0" );

    private static async Task ResponseShouldHaveIntroducedProblemDetails( DefaultHttpContext context, int statusCode, string introducedIn )
    {
        context.Response.ContentType.Should().Be( "application/problem+json" );
        context.Response.StatusCode.Should().Be( statusCode );

        var body = await ReadResponseBody( context );
        var problem = JsonDocument.Parse( body );
        var root = problem.RootElement;

        root.GetProperty( "type" ).GetString().Should().Be( "https://docs.api-versioning.org/problems#introduced" );
        root.GetProperty( "title" ).GetString().Should().Be( "API endpoint not yet introduced" );
        root.GetProperty( "status" ).GetInt32().Should().Be( statusCode );
        root.GetProperty( "detail" ).GetString().Should().Be(
            $"The HTTP resource that matches the request URI 'http://tempuri.org/api/values' was introduced in API version '{introducedIn}' and is not available in the requested version '1.0'." );
        root.GetProperty( "code" ).GetString().Should().Be( "EndpointNotIntroduced" );
    }

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

    private sealed class TestProblemDetailsService : IProblemDetailsService
    {
        public ValueTask WriteAsync( ProblemDetailsContext context ) => Write( context );

        public async ValueTask<bool> TryWriteAsync( ProblemDetailsContext context )
        {
            await Write( context );
            return true;
        }

        private static async ValueTask Write( ProblemDetailsContext context )
        {
            var response = context.HttpContext.Response;

            response.ContentType = "application/problem+json";

            await response.StartAsync();
            await JsonSerializer.SerializeAsync( response.Body, context.ProblemDetails );
        }
    }

    private sealed class TestApiVersionReporter : IReportApiVersions
    {
        public ApiVersionMapping Mapping => ApiVersionMapping.Explicit | ApiVersionMapping.Implicit;

        public void Report( HttpResponse response, ApiVersionModel apiVersionModel )
        {
            if ( apiVersionModel.SupportedApiVersions.Count > 0 )
            {
                response.Headers["api-supported-versions"] = string.Join( ", ", apiVersionModel.SupportedApiVersions );
            }

            if ( apiVersionModel.DeprecatedApiVersions.Count > 0 )
            {
                response.Headers["api-deprecated-versions"] = string.Join( ", ", apiVersionModel.DeprecatedApiVersions );
            }
        }
    }
}