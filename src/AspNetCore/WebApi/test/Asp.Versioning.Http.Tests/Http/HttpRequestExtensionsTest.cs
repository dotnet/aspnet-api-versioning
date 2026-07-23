// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

public class HttpRequestExtensionsTest
{
    [Fact]
    public void try_get_api_version_from_path_should_extract_from_route_pattern()
    {
        // arrange
        var pattern = RoutePatternFactory.Parse(
            "v{version:apiVersion}/test",
            default,
            new { version = new ApiVersionRouteConstraint() } );
        var patterns = new[] { pattern };
        var request = new Mock<HttpRequest>();

        request.SetupProperty( r => r.Path, new PathString( "/v2/test" ) );

        // act
        var matched = request.Object.TryGetApiVersionFromPath( patterns, "apiVersion", out var apiVersion );

        // assert
        matched.Should().BeTrue();
        apiVersion.Should().Be( "2" );
    }

    [Fact]
    public void try_get_api_version_from_path_should_infer_from_segment()
    {
        // arrange
        var patterns = Array.Empty<RoutePattern>();
        var services = new Mock<IServiceProvider>();
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();

        services.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( new ApiVersionParser() );
        context.SetupProperty( c => c.RequestServices, services.Object );
        request.SetupProperty( r => r.Path, new PathString( "/v2/test" ) );
        request.SetupGet( r => r.HttpContext ).Returns( context.Object );

        // act
        var matched = request.Object.TryGetApiVersionFromPath( patterns, "apiVersion", out var apiVersion );

        // assert
        matched.Should().BeTrue();
        apiVersion.Should().Be( "2" );
    }

    [Theory]
    [InlineData( "/v2/test" )]
    [InlineData( "/V2/test" )]
    [InlineData( "/api/v2/test" )]
    [InlineData( "/api/V2/test" )]
    public void try_get_api_version_from_path_should_fall_back_to_infer_from_segment( string path )
    {
        // arrange
        var pattern = RoutePatternFactory.Parse(
            "v1/test",
            default,
            new { version = new ApiVersionRouteConstraint() } );
        var patterns = new[] { pattern };
        var services = new Mock<IServiceProvider>();
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();

        services.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( new ApiVersionParser() );
        context.SetupProperty( c => c.RequestServices, services.Object );
        request.SetupProperty( r => r.Path, new PathString( path ) );
        request.SetupGet( r => r.HttpContext ).Returns( context.Object );

        // act
        var matched = request.Object.TryGetApiVersionFromPath( patterns, "apiVersion", out var apiVersion );

        // assert
        matched.Should().BeTrue();
        apiVersion.Should().Be( "2" );
    }

    [Fact]
    public void try_get_api_version_from_path_should_not_match()
    {
        // arrange
        var pattern = RoutePatternFactory.Parse(
            "v1/test",
            default,
            new { version = new ApiVersionRouteConstraint() } );
        var patterns = new[] { pattern };
        var services = new Mock<IServiceProvider>();
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();

        services.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( new ApiVersionParser() );
        context.SetupProperty( c => c.RequestServices, services.Object );
        request.SetupProperty( r => r.Path, new PathString( "/test" ) );
        request.SetupGet( r => r.HttpContext ).Returns( context.Object );

        // act
        var matched = request.Object.TryGetApiVersionFromPath( patterns, "apiVersion", out var apiVersion );

        // assert
        matched.Should().BeFalse();
        apiVersion.Should().BeNull();
    }

    [Fact]
    public void try_get_api_version_from_path_should_match_version_with_status()
    {
        // arrange
        var pattern = RoutePatternFactory.Parse(
            "v2-preview/test",
            default,
            new { version = new ApiVersionRouteConstraint() } );
        var patterns = new[] { pattern };
        var services = new Mock<IServiceProvider>();
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();

        services.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( new ApiVersionParser() );
        context.SetupProperty( c => c.RequestServices, services.Object );
        request.SetupProperty( r => r.Path, new PathString( "/v2-preview/test" ) );
        request.SetupGet( r => r.HttpContext ).Returns( context.Object );

        // act
        var matched = request.Object.TryGetApiVersionFromPath( patterns, "apiVersion", out var apiVersion );

        // assert
        matched.Should().BeTrue();
        apiVersion.Should().Be( "2-preview" );
    }
}