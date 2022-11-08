// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using static Asp.Versioning.ApiVersionProviderOptions;

public class RouteHandlerBuilderExtensionsTest
{
    [Fact]
    public void report_api_versions_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var reportApiVersions = default( IReportApiVersions );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       reportApiVersions = endpoint.Metadata.OfType<IReportApiVersions>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.ReportApiVersions();

        // assert
        reportApiVersions.Should().NotBeNull();
    }

    [Fact]
    public void is_api_version_neutral_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionNeutral = default( IApiVersionNeutral );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       versionNeutral = endpoint.Metadata.OfType<IApiVersionNeutral>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.IsApiVersionNeutral();

        // assert
        versionNeutral.Should().NotBeNull();
    }

    [Fact]
    public void has_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasApiVersion( 1.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = None,
                Versions = new[] { new ApiVersion( 1.0 ) },
            } );
    }

    [Fact]
    public void has_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasApiVersion( 1.0 );

        // assert
        versionSet.Build( new() ).SupportedApiVersions.Single().Should().Be( new ApiVersion( 1.0 ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasDeprecatedApiVersion( 0.9 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Deprecated,
                Versions = new[] { new ApiVersion( 0.9 ) },
            } );
    }

    [Fact]
    public void has_deprecated_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasDeprecatedApiVersion( 0.9 );

        // assert
        versionSet.Build( new() ).DeprecatedApiVersions.Single().Should().Be( new ApiVersion( 0.9 ) );
    }

    [Fact]
    public void advertises_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesApiVersion( 42.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Advertised,
                Versions = new[] { new ApiVersion( 42.0 ) },
            } );
    }

    [Fact]
    public void advertises_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesApiVersion( 42.0 );

        // assert
        versionSet.Build( new() ).SupportedApiVersions.Single().Should().Be( new ApiVersion( 42.0 ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesDeprecatedApiVersion( 42.0, "rc" );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Advertised | Deprecated,
                Versions = new[] { new ApiVersion( 42.0, "rc" ) },
            } );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesDeprecatedApiVersion( 42.0, "rc" );

        // assert
        versionSet.Build( new() ).DeprecatedApiVersions.Single().Should().Be( new ApiVersion( 42.0, "rc" ) );
    }

    [Fact]
    public void map_to_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.MapToApiVersion( 2.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Mapped,
                Versions = new[] { new ApiVersion( 2.0 ) },
            } );
    }
}